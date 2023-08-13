using System.Linq;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using VoidInventory.Content;

namespace VoidInventory
{
    public class VInventory
    {
        static Dictionary<Version, Action<VInventory, TagCompound>> LoadMethod = new();
        internal Dictionary<int, List<Item>> items = new();
        internal List<RecipeTask> recipeTasks = new();
        Task mergaTask = null;
        Queue<Item> mergaQueue = new();
        Dictionary<int, int> tileMap = new();
        internal bool HasWater, HasLava, HasHoney, HasShimmer;
        static VInventory()
        {
            LoadMethod[new Version(0, 0, 0, 1)] = Load_0001;
        }
        static List<Item> SplitItems(Item item)
        {
            List<Item> list = new();
            while (item.stack < item.maxStack)
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
                mergaTask = new(() => Merga_Inner(mergaQueue.Dequeue()));
                mergaTask.Start();
            }
        }
        /// <summary>
        /// 合并物品
        /// </summary>
        /// <param name="item"></param>
        public void Merga(ref Item item)
        {
            Item toInner = item;
            item = new(ItemID.None);
            int type = item.type;
            VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
            UIItemTex tex = new(type);
            var uiItems = VIUI.items;
            if (uiItems.TryAdd(type, tex))
            {
                int count = uiItems.Count;
                tex.SetPos(count % 6 * 56 + 10, count / 6 * 56 + 10);
                tex.Events.OnLeftClick += evt =>
                {
                    ui.rightView.ClearAllElements();
                    int j = 0;
                    foreach (Item i in items[type])
                    {
                        UIItemSlot slot = new(i)
                        {
                            CanTakeOutSlot = new(x => true)
                        };
                        slot.SetPos(j % 6 * 56 + 10, j / 6 * 56 + 10);
                        ui.rightView.AddElement(slot);
                        j++;
                    }
                };
                ui.leftView.AddElement(tex);
            }
            if (mergaTask is null)
            {
                //未有合并线程，创建并启动合并线程
                mergaTask = new(() => Merga_Inner(toInner));
                mergaTask.Start();
            }
            else
            {
                //已有合并线程，添加到任务队列
                mergaQueue.Enqueue(toInner);
            }
        }
        public void Merga_Inner(Item item, bool ignoreRecipe = false)
        {
            //将物品拆分(防止超出堆叠)
            List<Item> willPuts = SplitItems(item);
            if (items.TryGetValue(item.type, out List<Item> held))
            {
                //已有该类物品，尝试进行堆入
                int ptr;
                foreach (Item putItem in willPuts)
                {
                    //ptr表示正在进行堆入的索引，只能增加
                    //小于此索引的物品已经堆满，下一个物品堆入时直接跳过，以节省计算
                    //放在内部而不是foreach外部是因为无法确认ItemLoader.CanStack是否会出现变化
                    ptr = 0;
                    while (true)
                    {
                        //表示索引已经达到队尾，说明已经无处可堆，直接将切分的物品加入队尾
                        //此切分物品可能经过分堆，不是满堆，因此此时ptr不增加
                        if (ptr >= held.Count)
                        {
                            held.Add(putItem);
                            break;
                        }
                        Item container = held[ptr];
                        //满堆或无法接受堆叠则后移ptr
                        if (container.stack == container.maxStack || !ItemLoader.CanStack(container, putItem))
                        {
                            ptr++;
                            continue;
                        }
                        //计算移动堆叠数
                        int move = Math.Min(container.maxStack - container.stack, putItem.stack);
                        //容器增加堆叠
                        container.stack += move;
                        //物品减少堆叠
                        putItem.stack -= move;
                        //容器满堆则后移ptr
                        ptr += container.stack == container.maxStack ? 1 : 0;
                        //物品堆完，进入下一个要堆的物品
                        if (putItem.IsAir)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                //未有该种物品，直接将拆分结果设为储存
                items[item.type] = willPuts;
            }
            //检查合并任务队列是否清空
            if (mergaQueue.TryDequeue(out Item nextItem))
            {
                //继续合并
                Merga_Inner(nextItem);
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
            }
        }
        /// <summary>
        /// 将背包里所有物品进行合并(以压缩空间)
        /// 逻辑与<see cref="Merga_Inner(Item, bool)"/>相同
        /// </summary>
        public void MergaAllInInventory()
        {
            var buffer = items;
            items = new();
            foreach (var _items in buffer.Values)
            {
                foreach (var item in _items)
                {
                    if (items.TryGetValue(item.type, out List<Item> held))
                    {
                        int ptr = 0;
                        while (true)
                        {
                            if (ptr >= held.Count)
                            {
                                held.Add(item);
                                break;
                            }
                            Item container = held[ptr];
                            if (container.stack == container.maxStack || !ItemLoader.CanStack(container, item))
                            {
                                ptr++;
                                continue;
                            }
                            int move = Math.Min(container.maxStack - container.stack, item.stack);
                            container.stack += move;
                            item.stack -= move;
                            ptr += container.stack == container.maxStack ? 1 : 0;
                            if (item.IsAir)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        items[item.type] = new() { item };
                    }
                }
            }
        }
        /// <summary>
        /// 进行合成尝试
        /// </summary>
        void TryFinishRecipeTasks()
        {
            //刷新物块映射
            MapTileAsAdj();
            foreach (RecipeTask task in recipeTasks)
            {
                //如果完成了任意一次合成，说明背包物品发生变化，需要重新刷新物块映射并尝试合成
                task.TryFinish(this, false, out bool reTry, out _);
                if (reTry)
                {
                    TryFinishRecipeTasks();
                }
            }
        }
        /// <summary>
        /// 刷新物块映射
        /// </summary>
        void MapTileAsAdj()
        {
            tileMap.Clear();
            foreach (var pair in items.Values)
            {
                tileMap.Add(pair[0].createTile, pair.Sum(i => i.stack));
            }
            tileMap.Remove(-1);
            HasWater = items.ContainsKey(ItemID.WaterBucket) || items.ContainsKey(ItemID.BottomlessBucket);
            HasLava = items.ContainsKey(ItemID.LavaBucket) || items.ContainsKey(ItemID.BottomlessLavaBucket);
            HasHoney = items.ContainsKey(ItemID.HoneyBucket) || items.ContainsKey(ItemID.BottomlessHoneyBucket);
            HasShimmer = items.ContainsKey(ItemID.BottomlessShimmerBucket);
        }
        /// <summary>
        /// 返回是否持有指定类型的物品，并给出物品列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="heldItems"></param>
        /// <returns></returns>
        public bool HasItem(int type, out List<Item> heldItems)
        {
            if (items.TryGetValue(type, out heldItems))
            {
                return true;
            }
            return false;
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
            if (items.TryGetValue(type, out var held))
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
            tag[nameof(Version)] = "0.0.0.1";
            TagCompound sub = new();
            List<List<Item>> _items = new();
            foreach (var pair in items)
            {
                _items.Add(pair.Value);
            }
            sub[nameof(items)] = _items;
            tag[nameof(sub)] = sub;
        }
        internal void Load(TagCompound tag)
        {
            if (tag.TryGet(nameof(Version), out string version) && Version.TryParse(version, out var v) && LoadMethod.TryGetValue(v, out var loadMethod))
            {
                loadMethod(this, tag);
            }
        }
        static void Load_0001(VInventory inventory, TagCompound tag)
        {
            List<List<Item>> _items;
            if (!tag.TryGet("sub", out TagCompound sub) || !sub.TryGet(nameof(items), out _items))
            {
                return;
            }
            foreach (var list in _items)
            {
                list.ForEach(i => inventory.Merga_Inner(i, true));
            }
            inventory.MergaAllInInventory();
        }
        internal bool CountTile(int countNeed, params int[] tileNeeds)
        {
            foreach (var need in tileNeeds)
            {
                if (tileMap.TryGetValue(need, out var count))
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
    }
}