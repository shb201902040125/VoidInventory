using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VoidInventory
{
    public class VInventory
    {
        internal Dictionary<int, List<Item>> items = new();
        internal List<RecipeTask> recipeTasks = new();
        Task mergaTask = null;
        Queue<Item> mergaQueue = new();
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
        public void Merga(ref Item item)
        {
            Item toInner = item;
            item = new(ItemID.None);
            if (mergaTask is null)
            {
                mergaTask = new(() => Merga_Inner(toInner));
                mergaTask.Start();
            }
            else
            {
                mergaQueue.Enqueue(toInner);
            }
        }
        void Merga_Inner(Item item)
        {
            List<Item> willPuts = SplitItems(item);
            if (items.TryGetValue(item.type, out List<Item> held))
            {
                int ptr;
                foreach (Item putItem in willPuts)
                {
                    ptr = 0;
                    while (true)
                    {
                        if (ptr >= held.Count)
                        {
                            held.Add(putItem);
                            break;
                        }
                        Item container = held[ptr];
                        if (container.stack == container.maxStack || !ItemLoader.CanStack(container, putItem))
                        {
                            ptr++;
                            continue;
                        }
                        int move = Math.Min(container.maxStack - container.stack, putItem.stack);
                        container.stack += move;
                        putItem.stack -= move;
                        ptr++;
                        if (putItem.IsAir)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                items[item.type] = willPuts;
            }
            if (mergaQueue.TryDequeue(out Item nextItem))
            {
                Merga_Inner(nextItem);
            }
            else
            {
                TryFinishRecipeTasks();
                mergaTask = null;
            }
        }
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
                            ptr++;
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
        void TryFinishRecipeTasks()
        {
            foreach (RecipeTask task in recipeTasks)
            {
                task.TryFinish(items, out bool reTry, out _);
                if (reTry)
                {
                    TryFinishRecipeTasks();
                }
            }
        }
        public bool HasItem(int type, out List<Item> heldItems)
        {
            if (items.TryGetValue(type, out heldItems))
            {
                return true;
            }
            return false;
        }
        public bool TryPickOut(int type,int stackRequire,out int outCount, out List<Item> outItems)
        {
            outItems = new();
            int require = stackRequire;
            if (items.TryGetValue(type, out var held))
            {
                for(int i=held.Count-1;i>=0;i--)
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
    }
}