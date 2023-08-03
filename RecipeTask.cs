using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace VoidInventory
{
    public class RecipeTask
    {
        public Recipe RecipeTarget { get; }
        public bool CanSkip { get; internal set; }
        /// <summary>
        /// 合成任务状态
        /// <br>0:完成<see cref="CountTarget"/>次制作</br>
        /// <br>1:维持<see cref="CountTarget"/>个产物</br>
        /// <br>2:始终制作</br>
        /// </summary>
        public int TaskState = 0;
        public int CountTarget;
        static MethodInfo PlayerMeetsEnvironmentConditions;
        public RecipeTask(Recipe recipe, int state = 0, int count = -1)
        {
            RecipeTarget = recipe;
            TaskState = state;
            CountTarget = count > 0 ? (state == 0 ? 1 : (state == 1 ? 10 : -1)) : count;
        }
        internal void TryFinish(VInventory inv, out bool finishAtLeastOnce, out bool finishAll)
        {
            switch (TaskState)
            {
                case 0:
                    {
                        TryFinish_0(inv, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                case 1:
                    {
                        TryFinish_1(inv, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                case 2:
                    {
                        TryFinish_2(inv, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                default:
                    {
                        finishAtLeastOnce = false;
                        finishAll = false;
                        return;
                    }
            }
        }
        void TryFinish_0(VInventory inv, out bool finishAtLeastOnce, out bool finishAll)
        {
            finishAtLeastOnce = false;
            finishAll = true;
            if (CountTarget > 0)
            {
                while(DoRecipe(inv))
                {
                    finishAtLeastOnce = true;
                    finishAll = --CountTarget == 0;
                    if(finishAll)
                    {
                        return;
                    }
                }
            }
        }
        void TryFinish_1(VInventory inv, out bool finishAtLeastOnce, out bool finishAll)
        {
            int count = 0;
            if (inv.items.TryGetValue(RecipeTarget.createItem.type, out var held) && (count = held.Sum(i => i.stack)) >= CountTarget)
            {
                finishAtLeastOnce = false;
                finishAll = true;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            while(DoRecipe(inv))
            {
                count++;
                finishAtLeastOnce = true;
                finishAll = count == CountTarget;
                if(finishAll)
                {
                    return;
                }
            }
        }
        void TryFinish_2(VInventory inv, out bool finishAtLeastOnce, out bool finishAll)
        {
            finishAtLeastOnce = false;
            finishAll = false;
            while(DoRecipe(inv))
            {
                finishAtLeastOnce = true;
            }
        }
        bool DoRecipe(VInventory inv)
        {
            Player player = Main.LocalPlayer;
            if (RecipeTarget.requiredTile.All(tile =>
            {
                if (tile != -1)
                {
                    return player.adjTile[tile];
                }
                return true;
            }))
            {
                return false;
            }
            PlayerMeetsEnvironmentConditions ??= typeof(Recipe).GetMethod(nameof(PlayerMeetsEnvironmentConditions), BindingFlags.NonPublic | BindingFlags.Static);
            if(!(bool)PlayerMeetsEnvironmentConditions.Invoke(null,new object[] {player,RecipeTarget}))
            {
                return false;
            }
            Dictionary<int, int> used = new();
            int HowManyCanUse(int type,int howManyNeed,out Dictionary<int,int> useWhat)
            {
                int number = 0;
                useWhat = new();
                if(inv.items.TryGetValue(type , out var held))
                {
                    number = held.Sum(i => i.stack);
                    if(used.TryGetValue(type,out int useNumber))
                    {
                        number -= useNumber;
                    }
                    useWhat[type] = number;
                    if (howManyNeed - number <= 0)
                    {
                        useWhat[type] = howManyNeed;
                        goto toEnd;
                    }
                    howManyNeed -= number;
                }
                foreach (RecipeGroup group in RecipeGroup.recipeGroups.Values)
                {
                    foreach (int heldType in inv.items.Keys)
                    {
                        if (heldType == type)
                        {
                            continue;
                        }
                        if (group.ContainsItem(type) && group.ContainsItem(heldType))
                        {
                            number = inv.items[heldType].Sum(i => i.stack);
                            if (used.TryGetValue(heldType, out int useNumber))
                            {
                                number -= useNumber;
                            }
                            useWhat[heldType] = number;
                            if (howManyNeed - number <= 0)
                            {
                                useWhat[heldType] = howManyNeed;
                                goto toEnd;
                            }
                            howManyNeed -= number;
                        }
                    }
                }
            toEnd:;
                return useWhat.Sum(pair => pair.Value);
            }
            void ConsumeItems(Dictionary<int, int> useWhat)
            {
                foreach(var usetype in useWhat.Keys)
                {
                    var list = inv.items[usetype];
                    for(int i=list.Count-1; i>=0; i--)
                    {
                        int consume = Math.Min(list[i].stack, useWhat[usetype]);
                        list[i].stack -= consume;
                        useWhat[usetype] -= consume;
                        if (useWhat[usetype]==0)
                        {
                            break;
                        }
                    }
                    list.RemoveAll(i => i.IsAir);
                }
            }
            List<Dictionary<int, int>> useList = new();
            foreach (var required in RecipeTarget.requiredItem)
            {
                if (HowManyCanUse(required.type,required.stack, out var map) >= required.stack)
                {
                    useList.Add(map);
                }
                else
                {
                    return false;
                }
            }
            useList.ForEach(ConsumeItems);
            Item item = new(RecipeTarget.createItem.type, RecipeTarget.createItem.stack);
            inv.Merga(ref item);
            return true;
        }
    }
}
