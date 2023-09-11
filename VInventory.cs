using System.Linq;
using System.Threading.Tasks;
using Terraria.GameContent.UI;
using Terraria.ModLoader.IO;
using Terraria.UI;
using VoidInventory.Content;
using Terraria.ID;

namespace VoidInventory
{
    public class VInventory
    {
        internal Dictionary<int, List<Item>> _items = new();
        internal List<RecipeTask> recipeTasks = new();
        internal int tryDelay;
        /// <summary>
        /// 如果此值为True，UI应当不可交互
        /// </summary>
        internal bool Updating;
        private Task mergaTask = null;
        private Queue<Item> mergaQueue = new();
        private Dictionary<int, int> tileMap = new();
        internal bool HasWater, HasLava, HasHoney, HasShimmer;
        internal static bool needRefreshInv, needRefreshRT;
        static VInventory()
        {
        }

        private static List<Item> SplitItems(Item item)
        {
            List<Item> list = new();
            while (item.stack > item.maxStack)
            {
                Item newItem = new(item.type);
                int move = Math.Min(item.stack, newItem.maxStack);
                newItem.stack = move;
                item.stack -= move;
                list.Add(newItem);
            }
            list.Add(item);
            return list;
        }
        public void NormalUpdateCheck()
        {
            if (mergaTask is null && mergaQueue.Count > 0)
            {
                mergaTask = new(() => Merge_Inner(mergaQueue.Dequeue()));
                mergaTask.Start();
            }
            else
            {
                tryDelay++;
                if (tryDelay >= VIConfig.normalUpdateCheckTime * 60)
                {
                    TryFinishRecipeTasks();
                    CombineCurrency();
                    tryDelay = 0;
                }
            }
            if (needRefreshInv)
            {
                needRefreshInv = false;
                RefreshInvUI();
            }
            if (needRefreshRT)
            {
                needRefreshRT = false;
                RefreshTaskUI();
            }
        }
        /// <summary>
        /// 合并物品
        /// </summary>
        /// <param name="item"></param>
        public void Merge(ref Item item)
        {
            Item toInner = item;
            Merge_Inner(toInner);
            item = new(ItemID.None);
            //Item toInner = item;
            //if (mergaTask is null)
            //{
            //    //未有合并线程，创建并启动合并线程
            //    mergaTask = new(() => Merge_Inner(toInner));
            //    mergaTask.Start();
            //}
            //else
            //{
            //    //已有合并线程，添加到任务队列
            //    lock (mergaQueue)
            //    {
            //        mergaQueue.Enqueue(toInner);
            //    }
            //}
            //item = new(ItemID.None);
        }
        public void Merge_Inner(Item item, bool ignoreRecipe = false)
        {
            Updating = true;
            if (HasItem(item.type, out List<Item> held))
            {
                foreach (Item container in held)
                {
                    if (container.stack >= container.maxStack || !ItemLoader.CanStack(item, container))
                    {
                        continue;
                    }
                    int move = Math.Min(container.maxStack - container.stack, item.stack);
                    container.stack += move;
                    item.stack -= move;
                    if (item.stack == 0)
                    {
                        break;
                    }
                }
                if (item.stack > 0)
                {
                    held.AddRange(SplitItems(item));
                }
            }
            else
            {
                //未有该种物品，直接将拆分结果设为储存
                _items[item.type] = SplitItems(item);
            }
            //检查合并任务队列是否清空
            if (mergaQueue.TryDequeue(out Item nextItem))
            {
                //继续合并
                Merge_Inner(nextItem);
            }
            else
            {
                //加载物品时不进行合成尝试
                if (!ignoreRecipe)
                {
                    TryFinishRecipeTasks();
                }
                //清除合并线程
                mergaTask = null;
                tryDelay = 0;
                //进行刷新UI的回调
                needRefreshInv = true;
            }
            Updating = false;
        }
        internal void RefreshInvUI(Item lastItem = null)
        {
            if (Main.dedServ)
            {
                return;
            }
            VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
            if (ui.leftView == null) return;
            ui.leftView.ClearAllElements();
            List<int> keys = _items.Keys.ToList();
            keys.Sort();
            if (ui.focusFilter < 0)
            {
                ui.FindInvItem();
            }
            else ui.fbg.ChildrenElements.First(x => x is UIItemFilter filter && filter.Filter == ui.focusFilter).Events.LeftDown(ui);
            if (lastItem is not null && ui.focusType == lastItem.type)
            {
                ui.SortRight(_items[ui.focusType]);
            }
        }
        internal void RefreshTaskUI()
        {
            RTUI ui = VoidInventory.Ins.uis.Elements[RTUI.NameKey] as RTUI;
            if (ui is not null && ui.leftView is not null)
            {
                ui.LoadRT();
            }
        }
        /// <summary>
        /// 将背包里所有物品进行合并(以压缩空间)
        /// 逻辑与<see cref="Merge_Inner(Item, bool)"/>相同
        /// </summary>
        public void MergeAllInInventory()
        {
            Updating = true;
            Dictionary<int, List<Item>> buffer = _items;
            _items = new();
            foreach (List<Item> _items in buffer.Values)
            {
                foreach (Item item in _items)
                {
                    if (HasItem(item.type, out List<Item> held))
                    {
                        foreach (Item container in held)
                        {
                            if (container.stack >= container.maxStack || !ItemLoader.CanStack(item, container))
                            {
                                continue;
                            }
                            int move = Math.Min(container.maxStack - container.stack, item.stack);
                            container.stack += move;
                            item.stack -= move;
                            if (item.stack == 0)
                            {
                                break;
                            }
                        }
                        if (item.stack > 0)
                        {
                            held.AddRange(SplitItems(item));
                        }
                    }
                    else
                    {
                        this._items[item.type] = new() { item };
                    }
                }
            }
            _items.RemoveAll(type => !HasItem(type, out _));
            TryFinishRecipeTasks();
            CombineCurrency();
            needRefreshInv = true;
            needRefreshRT = true;
            Updating = false;
        }

        /// <summary>
        /// 进行合成尝试
        /// </summary>
        private void TryFinishRecipeTasks()
        {
            //刷新物块映射
            MapTileAsAdj();
            foreach (RecipeTask task in recipeTasks)
            {
                //如果完成了任意一次合成，说明背包物品发生变化，需要重新刷新物块映射并尝试合成
                task.TryFinish(this, false, out bool reTry, out bool report);
                if (task.TaskState == 0 && report && VIConfig.enableRecipeTaskReport)
                {
                    Main.NewText(task.GetReportMessage());
                }
                if (reTry)
                {
                    TryFinishRecipeTasks();
                    needRefreshInv = true;
                }
            }
        }

        /// <summary>
        /// 刷新物块映射
        /// </summary>
        private void MapTileAsAdj()
        {
            tileMap.Clear();
            foreach (List<Item> items in _items.Values)
            {
                if (!items.Any() || items[0].createTile == -1)
                {
                    continue;
                }
                tileMap[items[0].createTile] = items.Sum(i => i.stack);
            }
            HasWater = _items.ContainsKey(ItemID.WaterBucket) || _items.ContainsKey(ItemID.BottomlessBucket);
            HasLava = _items.ContainsKey(ItemID.LavaBucket) || _items.ContainsKey(ItemID.BottomlessLavaBucket);
            HasHoney = _items.ContainsKey(ItemID.HoneyBucket) || _items.ContainsKey(ItemID.BottomlessHoneyBucket);
            HasShimmer = _items.ContainsKey(ItemID.BottomlessShimmerBucket);
        }
        /// <summary>
        /// 返回是否持有指定类型的物品，并给出物品列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="heldItems"></param>
        /// <returns></returns>
        public bool HasItem(int type, out List<Item> heldItems)
        {
            return _items.TryGetValue(type, out heldItems) && heldItems.Sum(i => i.stack) > 0;
        }
        public bool HasItem(int type)
        {
            return _items.TryGetValue(type, out var heldItems) && heldItems.Sum(i => i.stack) > 0;
        }
        /// <summary>
        /// 从库存中提出指定类型的物品指定个数，返回是否完全提出成功。并给出提出数和提出物品列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stackRequire"></param>
        /// <param name="outCount"></param>
        /// <param name="outItems"></param>
        /// <returns></returns>
        public bool TryPickOut(int type, int stackRequire, out int outCount, out List<Item> outItems)
        {
            outItems = new();
            int require = stackRequire;
            if (HasItem(type, out List<Item> held))
            {
                for (int i = held.Count - 1; i >= 0; i--)
                {
                    int count = Math.Min(held[i].stack, stackRequire);
                    stackRequire -= count;
                    if (stackRequire > 0)
                    {
                        outItems.Add(held[i]);
                        held.RemoveAt(i);
                    }
                    else
                    {
                        outItems.Add(new(held[i].type, count));
                        held[i].stack -= count;
                        if (held[i].IsAir)
                        {
                            held.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
            outCount = require - stackRequire;
            return outCount == 0;
        }
        internal void Save(TagCompound tag)
        {
            tag["version"] = "0.0.0.1";
            List<Item> items = new();
            foreach (KeyValuePair<int, List<Item>> pair in _items)
            {
                if (pair.Value.Sum(i => i.stack) < 1)
                {
                    continue;
                }
                items.AddRange(pair.Value);
            }
            tag[nameof(_items)] = items;
            TagCompound taskTag = new();
            RecipeTask.Save(taskTag, recipeTasks);
            tag[nameof(recipeTasks)] = taskTag;
        }
        internal void Load(TagCompound tag)
        {
            if (tag.TryGet("version", out string version))
            {
                switch (version)
                {
                    case "0.0.0.1":
                        {
                            Load_0001(tag);
                            break;
                        }
                }
            }
        }
        private void Load_0001(TagCompound tag)
        {
            _items.Clear();
            if (tag.TryGet(nameof(_items), out List<Item> items))
            {
                foreach (Item item in items)
                {
                    if (_items.TryGetValue(item.type, out List<Item> items2))
                    {
                        items2.Add(item);
                    }
                    else
                    {
                        _items[item.type] = new() { item };
                    }
                }
                MergeAllInInventory();
            }
            if (tag.TryGet(nameof(recipeTasks), out TagCompound tasktag))
            {
                recipeTasks = RecipeTask.Load(tasktag);
                needRefreshRT = true;
            }
        }
        internal bool CountTile(int countNeed, params int[] tileNeeds)
        {
            foreach (int need in tileNeeds)
            {
                if (tileMap.TryGetValue(need, out int count))
                {
                    countNeed -= count;
                    if (countNeed <= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        internal List<Item> ToList()
        {
            List<Item> list = new();
            foreach (var items in _items.Values)
            {
                list.AddRange(items);
            }
            return list;
        }
        internal void CombineCurrency()
        {
            if (HasItem(ItemID.CopperCoin, out List<Item> helItems))
            {
                int count = helItems.Sum(i => i.stack);
                if (count > 100)
                {
                    TryPickOut(ItemID.CopperCoin, count - count % 100, out _, out _);
                    Item item = new(ItemID.SilverCoin, count / 100);
                    Merge_Inner(item, true);
                    needRefreshInv = true;
                }
            }
            if (HasItem(ItemID.SilverCoin, out helItems))
            {
                int count = helItems.Sum(i => i.stack);
                if (count > 100)
                {
                    TryPickOut(ItemID.SilverCoin, count - count % 100, out _, out _);
                    Item item = new(ItemID.GoldCoin, count / 100);
                    Merge_Inner(item, true);
                    needRefreshInv = true;
                }
            }
            if (HasItem(ItemID.GoldCoin, out helItems))
            {
                int count = helItems.Sum(i => i.stack);
                if (count > 100)
                {
                    TryPickOut(ItemID.GoldCoin, count - count % 100, out _, out _);
                    Item item = new(ItemID.PlatinumCoin, count / 100);
                    Merge_Inner(item, true);
                    needRefreshInv = true;
                }
            }
        }
        /// <summary>
        /// 筛选物品，注意筛选器顺序
        /// </summary>
        /// <param name="predicates"></param>
        /// <returns></returns>
        internal Dictionary<int, List<Item>> Filter(params Predicate<Item>[] predicates)
        {
            Dictionary<int, List<Item>> result = new();
            foreach (var pair in _items)
            {
                result[pair.Key] = pair.Value;
            }
            foreach (var predicate in predicates)
            {
                result.RemoveAll(list => list.Count == 0 || !predicate(list[0]));
            }
            return result;
        }

        internal class Hook
        {
            static bool loaded;
            internal static void LoadCurrencyHook()
            {
                if (loaded)
                {
                    return;
                }
                On_Player.CanAfford += On_Player_CanAfford;
                On_Player.PayCurrency += On_Player_PayCurrency;
                loaded = true;
            }

            private static bool On_Player_PayCurrency(On_Player.orig_PayCurrency orig, Player self, long price, int customCurrency)
            {
                return BuyItem(self, price, customCurrency, true);
            }

            private static bool On_Player_CanAfford(On_Player.orig_CanAfford orig, Player self, long price, int customCurrency)
            {
                return BuyItem(self, price, customCurrency, false);
            }

            internal static void UnloadCurrencyHook()
            {
                if (!loaded)
                {
                    return;
                }
                On_Player.CanAfford -= On_Player_CanAfford;
                On_Player.PayCurrency -= On_Player_PayCurrency;
                loaded = false;
            }
            private static bool BuyItem(Player self, long price, int customCurrency, bool realPay)
            {
                Main.NewText("do hook");
                VInventory inv = self.GetModPlayer<VIPlayer>().vInventory;
                Dictionary<int, int> valueMap;
                Dictionary<int, int> itemMap = new();
                if (customCurrency == -1)
                {
                    valueMap = new()
                    {
                        { ItemID.PlatinumCoin, 1000000 },
                        {ItemID.GoldCoin,10000 },
                        {ItemID.SilverCoin,100 },
                        {ItemID.CopperCoin,1 }
                    };
                }
                else
                {
                    CustomCurrencySystem system = ((Dictionary<int, CustomCurrencySystem>)typeof(CustomCurrencyManager).GetField("_currencies", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null))[customCurrency];
                    var list = ((Dictionary<int, int>)typeof(CustomCurrencySystem).GetField("_valuePerUnit").GetValue(system)).ToList();
                    list.Sort((p1, p2) => -p1.Value.CompareTo(p2.Value));
                    valueMap = new();
                    list.ForEach(p => valueMap[p.Key] = p.Value);
                }
                foreach (int type in valueMap.Keys)
                {
                    if (inv.HasItem(type, out var heldItems))
                    {
                        itemMap[type] = heldItems.Sum(i => i.stack);
                    }
                    else
                    {
                        itemMap[type] = 0;
                    }
                }
                Dictionary<int, int> used = new();
                int HowManyCanUse(int type)
                {
                    if (used.ContainsKey(type))
                    {
                        return itemMap[type] - used[type];
                    }
                    return itemMap[type];
                }
                void AddUsed(int type, int amount)
                {
                    if (used.ContainsKey(type))
                    {
                        used[type] += amount;
                    }
                    else
                    {
                        used[type] = amount;
                    }
                }
                List<int> types = valueMap.Keys.ToList();
                for (int index = 0; index < types.Count; index++)
                {
                    int type = types[index];
                    int canUse = HowManyCanUse(type);
                    int needUse = (int)(price / valueMap[type]);
                    if (canUse > needUse)
                    {
                        price -= (needUse + 1) * valueMap[type];
                        AddUsed(type, needUse + 1);
                        goto calculatedChange;
                    }
                    else if (canUse == needUse)
                    {
                        price -= needUse * valueMap[type];
                        AddUsed(type, needUse);
                        if (price == 0)
                        {
                            goto successPay;
                        }
                    }
                    else
                    {
                        price -= canUse * valueMap[type];
                        AddUsed(type, canUse);
                    }
                }
                return false;
            successPay:;
                if (!realPay)
                {
                    return true;
                }
                foreach (int type in used.Keys)
                {
                    inv.TryPickOut(type, used[type], out _, out _);
                }
                inv.MergeAllInInventory();
                return true;
            calculatedChange:;
                if (!realPay)
                {
                    return true;
                }
                foreach (int type in used.Keys)
                {
                    inv.TryPickOut(type, used[type], out _, out _);
                }
                price = Math.Abs(price);
                foreach (int type in valueMap.Keys)
                {
                    int count = (int)(price / valueMap[type]);
                    if (count > 0)
                    {
                        Item item = new(type, count);
                        inv.Merge(ref item);
                    }
                    price -= count * valueMap[type];
                    if (price == 0)
                    {
                        break;
                    }
                }
                inv.MergeAllInInventory();
                return true;
            }
        }
        internal class Filters
        {
            public static Predicate<Item> IsWeapon = i => i.damage > 0;
            public static Predicate<Item> IsTool = i => i.pick > 0 || i.hammer > 0 || i.axe > 0 || i.fishingPole > 0 || ItemIdsThatAreAcceptedAsTool.Contains(i.type);
            public static Predicate<Item> IsArmor = i => (i.headSlot != -1 || i.bodySlot != -1 || i.legSlot != -1) && !i.vanity;
            public static Predicate<Item> IsBuildingBlock = i => i.createWall != -1 || i.tileWand != -1 || (i.createTile != -1 && !Main.tileFrameImportant[i.createTile]);
            public static Predicate<Item> IsFurniture = i => i.createTile != -1 && Main.tileFrameImportant[i.createTile];
            public static Predicate<Item> IsAccessory = i => i.accessory && !ItemSlot.IsMiscEquipment(i);
            public static Predicate<Item> IsConsumable = i => i.type == ItemID.GuideVoodooDoll || i.type == ItemID.ClothierVoodooDoll || (i.consumable && !(i.createTile != -1 || i.createWall != -1 || i.tileWand != -1));
            public static Predicate<Item> IsMaterial = i => i.material;
            public static Predicate<Item> IsVanity = i => (i.headSlot != -1 || i.bodySlot != -1 || i.legSlot != -1) && i.vanity;
            public static Predicate<Item> IsMiscEquip = i => ItemSlot.IsMiscEquipment(i) && !i.accessory;
            private static HashSet<int> ItemIdsThatAreAcceptedAsTool = new()
            {
        509,
        850,
        851,
        3612,
        3625,
        3611,
        510,
        849,
        3620,
        1071,
        1543,
        1072,
        1544,
        1100,
        1545,
        50,
        3199,
        3124,
        5358,
        5359,
        5360,
        5361,
        5437,
        1326,
        5335,
        3384,
        4263,
        4819,
        4262,
        946,
        4707,
        205,
        206,
        207,
        1128,
        3031,
        4820,
        5302,
        5364,
        4460,
        4608,
        4872,
        3032,
        5303,
        5304,
        1991,
        4821,
        3183,
        779,
        5134,
        1299,
        4711,
        4049,
        114
    };
            public static Predicate<Item> Misc(params Predicate<Item>[] predicates)
            {
                return i => !predicates.Any(p => p(i));
            }
        }
    }
}