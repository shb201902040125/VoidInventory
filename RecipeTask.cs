using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader.IO;

namespace VoidInventory
{
    public class RecipeTask
    {
        static Dictionary<Version, Action<RecipeTask, TagCompound>> LoadMethod = new();
        static HashSet<Condition> ignores = new();
        static RecipeTask()
        {
            FieldInfo[] fs = typeof(Condition).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fi in fs)
            {
                if (fi.FieldType == typeof(Condition))
                {
                    ignores.Add((Condition)fi.GetValue(null));
                }
            }
            LoadMethod[new Version(0, 0, 0, 1)] = Load_0001;
        }
        public Recipe RecipeTarget { get; }
        public bool Stopping { get; internal set; }
        /// <summary>
        /// 合成任务状态
        /// <br>0:完成<see cref="CountTarget"/>次制作</br>
        /// <br>1:维持<see cref="CountTarget"/>个产物</br>
        /// <br>2:始终制作</br>
        /// </summary>
        public int TaskState = 0;
        public int CountTarget;
        internal Dictionary<int, Dictionary<int, bool>> exclude = new();
        public RecipeTask(Recipe recipe, int state = 0, int count = -1)
        {
            RecipeTarget = recipe;
            TaskState = state;
            CountTarget = count > 0 ? (state == 0 ? 1 : (state == 1 ? 10 : -1)) : count;
        }
        /// <summary>
        /// 尝试完成合成
        /// </summary>
        /// <param name="inv"></param>
        /// <param name="fake"></param>
        /// <param name="finishAtLeastOnce"></param>
        /// <param name="finishAll"></param>
        internal void TryFinish(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            //暂停直接跳过
            if (Stopping)
            {
                finishAtLeastOnce = false;
                finishAll = false;
                return;
            }
            switch (TaskState)
            {
                case 0:
                    {
                        TryFinish_0(inv, fake, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                case 1:
                    {
                        TryFinish_1(inv, fake, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                case 2:
                    {
                        TryFinish_2(inv, fake, out finishAtLeastOnce, out finishAll);
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
        void TryFinish_0(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = CountTarget == 0;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = true;
            if (CountTarget > 0)
            {
                //合成成功，完成一次标记为true，全部完成判断次数需求是否到0
                while (DoRecipe(inv))
                {
                    finishAtLeastOnce = true;
                    finishAll = --CountTarget == 0;
                    if (finishAll)
                    {
                        return;
                    }
                }
            }
        }
        void TryFinish_1(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            int count = 0;
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = inv.items.TryGetValue(RecipeTarget.createItem.type, out var fakeHeld) && (count = fakeHeld.Sum(i => i.stack)) >= CountTarget;
                return;
            }
            //统计背包里目标产物个数是否达到
            if (inv.items.TryGetValue(RecipeTarget.createItem.type, out var held) && (count = held.Sum(i => i.stack)) >= CountTarget)
            {
                finishAtLeastOnce = false;
                finishAll = true;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            while (DoRecipe(inv))
            {
                count++;
                finishAtLeastOnce = true;
                finishAll = count == CountTarget;
                if (finishAll)
                {
                    return;
                }
            }
        }
        void TryFinish_2(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = false;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            while (DoRecipe(inv))
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
            if (!CheckTiles(player, inv))
            {
                return false;
            }
            if (!CheckConditions(player, inv))
            {
                return false;
            }
            Dictionary<int, int> used = new();
            List<Dictionary<int, int>> useList = new();
            foreach (var required in RecipeTarget.requiredItem)
            {
                if (HowManyCanUse(required.type, required.stack, out var map, inv, used) >= required.stack)
                {
                    useList.Add(map);
                }
                else
                {
                    return false;
                }
            }
            useList.ForEach(useWhat => ConsumeItems(useWhat, inv));
            Item item = new(RecipeTarget.createItem.type, RecipeTarget.createItem.stack);
            inv.Merga(ref item);
            return true;
        }
        bool CanRecipe(VInventory inv)
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
            if (!CheckTiles(player, inv))
            {
                return false;
            }
            if (!CheckConditions(player, inv))
            {
                return false;
            }
            Dictionary<int, int> used = new();
            List<Dictionary<int, int>> useList = new();
            foreach (var required in RecipeTarget.requiredItem)
            {
                if (HowManyCanUse(required.type, required.stack, out _, inv, used) < required.stack)
                {
                    return false;
                }
            }
            return true;
        }
        private static void ConsumeItems(Dictionary<int, int> useWhat, VInventory inv)
        {
            foreach (var usetype in useWhat.Keys)
            {
                var list = inv.items[usetype];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    int consume = Math.Min(list[i].stack, useWhat[usetype]);
                    list[i].stack -= consume;
                    useWhat[usetype] -= consume;
                    if (useWhat[usetype] == 0)
                    {
                        break;
                    }
                }
                list.RemoveAll(i => i.IsAir);
            }
        }
        private int HowManyCanUse(int type, int howManyNeed, out Dictionary<int, int> useWhat, VInventory inv, Dictionary<int, int> used)
        {
            int number = 0;
            useWhat = new();
            if (inv.items.TryGetValue(type, out var held))
            {
                number = held.Sum(i => i.stack);
                if (used.TryGetValue(type, out int useNumber))
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
                bool checkSkip = exclude.TryGetValue(group.RegisteredId, out var skip);
                foreach (int heldType in inv.items.Keys)
                {
                    if (heldType == type || (checkSkip && skip.TryGetValue(heldType, out bool skipThis) && skipThis))
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
        bool CheckTiles(Player player, VInventory inv)
        {
            foreach (var requiredTile in RecipeTarget.requiredTile)
            {
                if (!player.adjTile[requiredTile] && !inv.CountTile(1, requiredTile))
                {
                    return false;
                }
            }
            return true;
        }
        bool CheckConditions(Player player, VInventory inv)
        {
            if (RecipeTarget.Conditions.Contains(Condition.NearWater) && !(player.adjWater || inv.HasWater))
            {
                return false;
            }
            if (RecipeTarget.Conditions.Contains(Condition.NearLava) && !(player.adjLava || inv.HasLava))
            {
                return false;
            }
            if (RecipeTarget.Conditions.Contains(Condition.NearHoney) && !(player.adjHoney || inv.HasHoney))
            {
                return false;
            }
            if (RecipeTarget.Conditions.Contains(Condition.NearShimmer) && !(player.adjShimmer || inv.HasShimmer))
            {
                return false;
            }
            if (RecipeTarget.Conditions.Contains(Condition.InSnow) && !(player.ZoneSnow || inv.CountTile(1500, TileID.SnowBlock, TileID.IceBlock)))
            {
                return false;
            }
            if (RecipeTarget.Conditions.Contains(Condition.InGraveyard) && !(player.ZoneGraveyard || inv.CountTile(7, TileID.Tombstones)))
            {
                return false;
            }
            foreach (var condition in RecipeTarget.Conditions)
            {
                if (ignores.Contains(condition))
                {
                    continue;
                }
                if (!condition.IsMet())
                {
                    return false;
                }
            }
            return true;
        }
        public void Save(TagCompound tag)
        {
            tag[nameof(Version)] = "0.0.0.1";
            //TODO
        }
        public void Load(TagCompound tag)
        {
            if (tag.TryGet(nameof(Version), out string version) && Version.TryParse(version, out var v) && LoadMethod.TryGetValue(v, out var loadMethod))
            {
                loadMethod(this, tag);
            }
        }
        public static void Load_0001(RecipeTask task, TagCompound tag)
        {
            //TODO
        }
    }
}