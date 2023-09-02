using System.Linq;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using VoidInventory.Content;
using VoidInventory.Filters;

namespace VoidInventory
{
    public class VInventory
    {
        internal static Filter<Item, IEnumerable<Item>> currentFilter;
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
                mergaTask = new(() => Merga_Inner(mergaQueue.Dequeue()));
                mergaTask.Start();
            }
            else
            {
                tryDelay++;
                if (tryDelay == 300)
                {
                    TryFinishRecipeTasks();
                    tryDelay = 0;
                }
            }
        }
        /// <summary>
        /// 合并物品
        /// </summary>
        /// <param name="item"></param>
        public void Merga(ref Item item)
        {
            Item toInner = item;
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
            item = new(ItemID.None);
        }
        public void Merga_Inner(Item item, bool ignoreRecipe = false)
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
                tryDelay = 0;
                //进行刷新UI的回调
                RefreshUI(item);
            }
        }

        private void RefreshUI(Item lastItem = null, Filter<Item, IEnumerable<Item>> filter = null)
        {
            currentFilter = filter ?? currentFilter;
            List<Item> forUI = new();
            if (currentFilter is null)
            {
                foreach (KeyValuePair<int, List<Item>> items in _items)
                {
                    forUI.AddRange(items.Value);
                }
            }
            else
            {
                foreach (KeyValuePair<int, List<Item>> pair in _items)
                {
                    forUI.AddRange(currentFilter.FilterItems(pair.Value));
                }
            }
            //用forUI刷新UI界面
            VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
            UIItemTex tex;
            ui.leftView.ClearAllElements();
            var keys = _items.Keys.ToList();
            keys.Sort();
            int count = 0;
            foreach (var key in keys)
            {
                tex = new(key);
                tex.SetPos((count % 6 * 56) + 10, (count / 6 * 56) + 10);
                ui.LoadClickEvent(tex, key, _items[key]);
                ui.leftView.AddElement(tex);
                count++;
            }
            if (lastItem is not null && ui.focusType == lastItem.type)
            {
                ui.SortRight(_items[ui.focusType]);
            }
            Updating = false;
        }
        /// <summary>
        /// 将背包里所有物品进行合并(以压缩空间)
        /// 逻辑与<see cref="Merga_Inner(Item, bool)"/>相同
        /// </summary>
        public void MergaAllInInventory()
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
            RefreshUI();
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
                if (task.TaskState == 0 && report)
                {
                    Main.NewText(task.GetReportMessage());
                }
                if (reTry)
                {
                    TryFinishRecipeTasks();
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
            foreach (var pair in _items)
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
            if(tag.TryGet("version",out string version))
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
            else
            {
                VoidInventory.Ins.Logger.Debug("Lost Data:VInventory.Load");
            }
        }
        private void Load_0001(TagCompound tag)
        {
            _items.Clear();
            if(tag.TryGet(nameof(_items),out List<Item> items))
            {
                foreach (var item in items)
                {
                    if(_items.TryGetValue(item.type,out List<Item> items2))
                    {
                        items2.Add(item);
                    }
                    else
                    {
                        _items[item.type] = new() { item };
                    }
                }
                MergaAllInInventory();
            }
            if (tag.TryGet(nameof(recipeTasks),out TagCompound tasktag))
            {
                recipeTasks = RecipeTask.Load(tasktag);
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
    }
}