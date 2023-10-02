using System.Linq;
using Terraria.GameContent.UI;
using Terraria.ModLoader.IO;

namespace VoidInventory.Reverse
{
    public class VInventory_R
    {
        internal readonly Dictionary<int, List<Item>> _storge = new();
        internal readonly Dictionary<int, int> _tileMap = new();
        internal List<RecipeTask_R> _tasks = new();
        public bool HasWater { get; private set; }
        public bool HasLava { get; private set; }
        public bool HasHoney { get; private set; }
        public bool HasShimmer { get; private set; }
        public void Merge(ref Item item)
        {
            if (item is null)
            {
                return;
            }
            Item processing = item;
            item = new(ItemID.None);
            processing.favorited = false;
            if (_storge.TryGetValue(processing.type, out List<Item> heldItems))
            {
                heldItems.RemoveAll(i => i.IsAir);
                if (heldItems.Count > 0)
                {
                    heldItems.Add(processing);
                    Combine(heldItems);
                }
                else
                {
                    MergeToEmptySlot(processing);
                }
            }
            else
            {
                MergeToEmptySlot(processing);
            }
        }
        internal void MergeToEmptySlot(Item item)
        {
            List<Item> heldItems = new();
            while (item.stack > item.maxStack)
            {
                Item item2 = item.Clone();
                item2.stack = item2.maxStack;
                item.stack -= item2.stack;
                heldItems.Add(item2);
            }
            heldItems.Add(item);
            _storge[item.type] = heldItems;
        }

        internal static void Combine(List<Item> combineList)
        {
            if (combineList.Count < 2)
            {
                return;
            }
            int ptr0 = 0, ptr1 = combineList.Count - 1;
            Item currentItem, usingItem;
            while (true)
            {
                if (ptr0 >= combineList.Count)
                {
                    break;
                }
                currentItem = combineList[ptr0];
                if (currentItem.stack == currentItem.maxStack)
                {
                    ptr0++;
                    if (ptr0 == combineList.Count)
                    {
                        break;
                    }
                    ptr1 = combineList.Count - 1;
                    continue;
                }
                if (currentItem.stack > currentItem.maxStack)
                {
                    Item move = currentItem.Clone();
                    move.stack = currentItem.stack - currentItem.maxStack;
                    currentItem.stack -= move.stack;
                    combineList.Add(move);
                    ptr0++;
                    ptr1 = combineList.Count - 1;
                    continue;
                }
                else
                {
                    if (ptr0 == ptr1)
                    {
                        ptr0++;
                        ptr1 = combineList.Count - 1;
                    }
                    usingItem = combineList[ptr1];
                    int moveAmount = ItemLoader.CanStack(currentItem, usingItem) ? Math.Min(currentItem.maxStack - currentItem.stack, usingItem.stack) : 0;
                    currentItem.stack += moveAmount;
                    usingItem.stack -= moveAmount;
                    if (usingItem.stack == 0)
                    {
                        combineList.RemoveAt(ptr1);
                    }
                    if (currentItem.stack == currentItem.maxStack)
                    {
                        ptr0++;
                        if (ptr0 == combineList.Count)
                        {
                            break;
                        }
                        ptr1 = combineList.Count - 1;
                        continue;
                    }
                    else
                    {
                        ptr1--;
                    }
                }
            }
        }

        internal void RefreshTileMap()
        {
            _tileMap.Clear();
            foreach (KeyValuePair<int, List<Item>> pair in _storge)
            {
                Item item = pair.Value.FirstOrDefault(i => !i.IsAir, null);
                if (item is null)
                {
                    continue;
                }
                if (item.createTile == -1)
                {
                    switch (pair.Key)
                    {
                        case ItemID.WaterBucket:
                        case ItemID.BottomlessBucket:
                            HasWater = true; break;
                        case ItemID.LavaBucket:
                        case ItemID.BottomlessLavaBucket:
                            HasLava = true; break;
                        case ItemID.HoneyBucket:
                        case ItemID.BottomlessHoneyBucket:
                            HasHoney = true; break;
                        case ItemID.BottomlessShimmerBucket:
                            HasHoney = true; break;
                    }
                }
                else
                {
                    if (_tileMap.ContainsKey(pair.Key))
                    {
                        _tileMap[item.createTile] += pair.Value.Sum(i => i.stack);
                    }
                    else
                    {
                        _tileMap[item.createTile] = pair.Value.Sum(i => i.stack);
                    }
                }
            }
        }

        internal void TryFinishRecipeTask()
        {
            RefreshTileMap();
            foreach (RecipeTask_R task in _tasks)
            {
                task.TryFinsh(this, false, out bool retry, out bool successful);
                if (retry)
                {
                    TryFinishRecipeTask();
                }
                else if (successful)
                {
                    //TODO
                }
            }
        }
        public bool HasItem(int type)
        {
            return _storge.TryGetValue(type, out List<Item> items) && items.Sum(i => i.stack) > 0;
        }

        internal bool HasItem(int type, out List<Item> heldItems)
        {
            return _storge.TryGetValue(type, out heldItems) && heldItems.Sum(i => i.stack) > 0;
        }
        internal void Save(TagCompound tag)
        {
            tag["version"] = "0.0.0.1";
            List<Item> items = new();
            foreach (KeyValuePair<int, List<Item>> pair in _storge)
            {
                if (pair.Value.Sum(i => i.stack) < 1)
                {
                    continue;
                }
                items.AddRange(pair.Value);
            }
            tag[nameof(_storge)] = items;
            TagCompound taskTag = new();
            RecipeTask_R.Save(taskTag, _tasks);
            tag[nameof(_tasks)] = taskTag;
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
        internal void Load_0001(TagCompound tag)
        {
            _storge.Clear();
            if (tag.TryGet(nameof(_storge), out List<Item> items))
            {
                foreach (Item item in items)
                {
                    if (_storge.TryGetValue(item.type, out List<Item> items2))
                    {
                        items2.Add(item);
                    }
                    else
                    {
                        _storge[item.type] = new() { item };
                    }
                }
                foreach (List<Item> list in _storge.Values)
                {
                    Combine(list);
                }
            }
            if (tag.TryGet(nameof(_tasks), out TagCompound tasktag))
            {
                _tasks = RecipeTask_R.Load(tasktag);
            }
        }
        internal bool CountTile(int countNeed, params int[] tileNeeds)
        {
            foreach (int need in tileNeeds)
            {
                if (_tileMap.TryGetValue(need, out int count))
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
                if (!held.Any())
                {
                    _storge.Remove(type);
                }
            }
            outCount = require - stackRequire;
            return outCount == 0;
        }
        internal class Hook
        {
            internal static bool loaded;
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
            internal static bool On_Player_PayCurrency(On_Player.orig_PayCurrency orig, Player self, long price, int customCurrency)
            {
                return BuyItem(self, price, customCurrency, true);
            }

            internal static bool On_Player_CanAfford(On_Player.orig_CanAfford orig, Player self, long price, int customCurrency)
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
            internal static bool BuyItem(Player self, long price, int customCurrency, bool realPay)
            {
                VInventory_R inv = self.GetModPlayer<VIPlayer_R>().vInventory;
                Dictionary<int, int> valueMap;
                Dictionary<int, int> itemMap = new();
                if (customCurrency == -1)
                {
                    valueMap = new()
                    {
                        {ItemID.PlatinumCoin, 1000000 },
                        {ItemID.GoldCoin,10000 },
                        {ItemID.SilverCoin,100 },
                        {ItemID.CopperCoin,1 }
                    };
                }
                else
                {
                    CustomCurrencySystem system = ((Dictionary<int, CustomCurrencySystem>)typeof(CustomCurrencyManager).GetField("_currencies", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null))[customCurrency];
                    List<KeyValuePair<int, int>> list = ((Dictionary<int, int>)typeof(CustomCurrencySystem).GetField("_valuePerUnit").GetValue(system)).ToList();
                    list.Sort((p1, p2) => -p1.Value.CompareTo(p2.Value));
                    valueMap = new();
                    list.ForEach(p => valueMap[p.Key] = p.Value);
                }
                foreach (int type in valueMap.Keys)
                {
                    itemMap[type] = inv.HasItem(type, out List<Item> heldItems) ? heldItems.Sum(i => i.stack) : 0;
                }
                Dictionary<int, int> used = new();
                int HowManyCanUse(int type)
                {
                    return used.ContainsKey(type) ? itemMap[type] - used[type] : itemMap[type];
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
                    if (inv._storge.TryGetValue(type, out List<Item> held))
                    {
                        Combine(held);
                    }
                }
                return true;
            calculatedChange:;
                if (!realPay)
                {
                    return true;
                }
                foreach (int type in used.Keys)
                {
                    inv.TryPickOut(type, used[type], out _, out _);
                    if (inv._storge.TryGetValue(type, out List<Item> held))
                    {
                        Combine(held);
                    }
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
                return true;
            }
        }
    }
}