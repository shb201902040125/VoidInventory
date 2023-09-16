using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace VoidInventory.Reverse
{
    public class RecipeTask_R
    {
        private static HashSet<Condition> ignores = new();
        static RecipeTask_R()
        {
            FieldInfo[] fs = typeof(Condition).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fi in fs)
            {
                if (fi.FieldType == typeof(Condition))
                {
                    ignores.Add((Condition)fi.GetValue(null));
                }
            }
        }
        public enum RecipeTaskState : int
        {
            CompleteTargetCountTimes,
            KeepTargetCountProduct,
            Always
        }
        public RecipeTask_R(Recipe recipe, RecipeTaskState taskState = RecipeTaskState.CompleteTargetCountTimes, int count = -1)
        {
            Target = recipe;
            TaskState = taskState;
            SetDefaultCount();
            if (count > 0 && TaskState != RecipeTaskState.Always)
            {
                Count = count;
            }
        }
        public Recipe Target { get; private set; }
        public RecipeTaskState TaskState { get; private set; }
        public int Count { get; private set; }
        public bool Stopping { get; private set; }

        private Dictionary<int, Dictionary<int, bool>> exclude = new();

        private void SetDefaultCount()
        {
            switch (TaskState)
            {
                case RecipeTaskState.CompleteTargetCountTimes:
                    {
                        Count = 1;
                        break;
                    }
                case RecipeTaskState.KeepTargetCountProduct:
                    {
                        Count = 10;
                        break;
                    }
                case RecipeTaskState.Always:
                    {
                        Count = -1;
                        break;
                    }
            }
        }
        public void SetState(RecipeTaskState state, int count = -1)
        {
            if (TaskState != state)
            {
                TaskState = state;
                SetDefaultCount();
            }
            if (count > 0 && TaskState != RecipeTaskState.Always)
            {
                Count = count;
            }
        }
        internal void TryFinsh(VInventory_R inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            if (Stopping)
            {
                fake = true;
            }
            switch (TaskState)
            {
                case RecipeTaskState.CompleteTargetCountTimes:
                    {
                        TryFinish_0(inv, fake, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                case RecipeTaskState.KeepTargetCountProduct:
                    {
                        TryFinish_1(inv, fake, out finishAtLeastOnce, out finishAll);
                        return;
                    }
                case RecipeTaskState.Always:
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
        private void TryFinish_0(VInventory_R inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = Count == 0;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            if (Count > 0)
            {
                //合成成功，完成一次标记为true，全部完成判断次数需求是否到0
                int times = 0;
                while (DoRecipe(inv))
                {
                    times++;
                    finishAtLeastOnce = true;
                    finishAll = --Count == 0;
                    if (finishAll)
                    {
                        break;
                    }
                }
                if (times > 0)
                {
                    Item item = new(Target.createItem.type, times * Target.createItem.stack);
                    inv.Merge(ref item);
                }
            }
        }
        private void TryFinish_1(VInventory_R inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            int count = 0;
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = inv._storge.TryGetValue(Target.createItem.type, out List<Item> fakeHeld) && (count = fakeHeld.Sum(i => i.stack)) >= Count;
                return;
            }
            //统计背包里目标产物个数是否达到
            bool flag = inv._storge.TryGetValue(Target.createItem.type, out List<Item> held);
            if (!flag && Count == 0)
            {
                finishAtLeastOnce = false;
                finishAll = true;
                return;
            }
            if (flag && (count = held.Sum(i => i.stack)) >= Count)
            {
                finishAtLeastOnce = false;
                finishAll = true;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            int times = 0;
            while (DoRecipe(inv))
            {
                times++;
                count += Target.createItem.stack;
                finishAtLeastOnce = true;
                finishAll = count >= Count;
                if (finishAll)
                {
                    break;
                }
            }
            if (times > 0)
            {
                Item item = new(Target.createItem.type, times * Target.createItem.stack);
                inv.Merge(ref item);
            }
        }
        private void TryFinish_2(VInventory_R inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = false;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            int times = 0;
            while (DoRecipe(inv))
            {
                times++;
                finishAtLeastOnce = true;
            }
            if (times > 0)
            {
                Item item = new(Target.createItem.type, times * Target.createItem.stack);
                inv.Merge(ref item);
            }
        }
        private bool DoRecipe(VInventory_R inv)
        {
            Player player = Main.LocalPlayer;
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
            foreach (Item required in Target.requiredItem)
            {
                if (HowManyCanUse(required.type, required.stack, out Dictionary<int, int> map, inv, used) >= required.stack)
                {
                    useList.Add(map);
                }
                else
                {
                    return false;
                }
            }
            useList.ForEach(useWhat => ConsumeItems(useWhat, inv));
            return true;
        }
        private bool CanRecipe(VInventory_R inv)
        {
            Player player = Main.LocalPlayer;
            if (!CheckTiles(player, inv))
            {
                return false;
            }
            if (!CheckConditions(player, inv))
            {
                return false;
            }
            Dictionary<int, int> used = new();
            foreach (Item required in Target.requiredItem)
            {
                if (HowManyCanUse(required.type, required.stack, out _, inv, used) < required.stack)
                {
                    return false;
                }
            }
            return true;
        }
        private static void ConsumeItems(Dictionary<int, int> useWhat, VInventory_R inv)
        {
            foreach (int usetype in useWhat.Keys)
            {
                List<Item> list = inv._storge[usetype];
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
        public int HowManyCanUse(int type, int howManyNeed, out Dictionary<int, int> useWhat, VInventory_R inv, Dictionary<int, int> used)
        {
            int number = 0;
            useWhat = new();
            if (inv._storge.TryGetValue(type, out List<Item> held))
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
                bool checkSkip = exclude.TryGetValue(group.RegisteredId, out Dictionary<int, bool> skip);
                foreach (int heldType in inv._storge.Keys)
                {
                    if (heldType == type || (checkSkip && skip.TryGetValue(heldType, out bool skipThis) && skipThis))
                    {
                        continue;
                    }
                    if (group.ContainsItem(type) && group.ContainsItem(heldType))
                    {
                        number = inv._storge[heldType].Sum(i => i.stack);
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
        private bool CheckTiles(Player player, VInventory_R inv)
        {
            foreach (int requiredTile in Target.requiredTile)
            {
                if (requiredTile == -1)
                {
                    continue;
                }
                if (!player.adjTile[requiredTile] && !inv.CountTile(1, requiredTile))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool CheckTile(int tileID, Player player, VInventory_R inv)
        {
            return player.adjTile[tileID] || inv.CountTile(1, tileID);
        }
        private bool CheckConditions(Player player, VInventory_R inv)
        {
            if (Target.Conditions.Contains(Condition.NearWater) && !(player.adjWater || inv.HasWater))
            {
                return false;
            }
            if (Target.Conditions.Contains(Condition.NearLava) && !(player.adjLava || inv.HasLava))
            {
                return false;
            }
            if (Target.Conditions.Contains(Condition.NearHoney) && !(player.adjHoney || inv.HasHoney))
            {
                return false;
            }
            if (Target.Conditions.Contains(Condition.NearShimmer) && !(player.adjShimmer || inv.HasShimmer))
            {
                return false;
            }
            if (Target.Conditions.Contains(Condition.InSnow) && !(player.ZoneSnow || inv.CountTile(1500, TileID.SnowBlock, TileID.IceBlock)))
            {
                return false;
            }
            if (Target.Conditions.Contains(Condition.InGraveyard) && !(player.ZoneGraveyard || inv.CountTile(7, TileID.Tombstones)))
            {
                return false;
            }
            foreach (Condition condition in Target.Conditions)
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
        internal static void Save(TagCompound tag, List<RecipeTask_R> tasks)
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream);
            writer.Write("0.0.0.1");
            writer.Write(tasks.Count);
            tasks.ForEach(task =>
            {
                writer.Write(task.Target.GetCheckCode());
                writer.Write((int)task.TaskState);
                writer.Write(task.Count);
                writer.Write(task.Stopping);
                writer.Write(task.exclude.Count);
                foreach (KeyValuePair<int, Dictionary<int, bool>> pair in task.exclude)
                {
                    writer.Write(RecipeGroup.recipeGroups[pair.Key].GetText());
                    writer.Write(pair.Value.Count);
                    foreach (KeyValuePair<int, bool> subpair in pair.Value)
                    {
                        writer.Write(ItemLoader.GetItem(subpair.Key)?.FullName ?? (subpair.Key.ToString()));
                        writer.Write(subpair.Value);
                    }
                }
            });
            tag["RTS"] = stream.ToArray();
        }
        public static List<RecipeTask_R> Load(TagCompound tag)
        {
            if (tag.TryGet("RTS", out byte[] data))
            {
                Dictionary<string, Recipe> rs = new();
                Dictionary<string, int> rgs = new();
                foreach (Recipe r in Main.recipe[0..Recipe.maxRecipes])
                {
                    rs[r.GetCheckCode()] = r;
                }
                foreach (KeyValuePair<int, RecipeGroup> rg in RecipeGroup.recipeGroups)
                {
                    rgs[rg.Value.GetText()] = rg.Key;
                }
                using MemoryStream stream = new(data);
                using BinaryReader reader = new(stream);
                string version = reader.ReadString();
                return version switch
                {
                    "0.0.0.1" => Load_0001(reader, rs, rgs),
                    _ => new(),
                };
            }
            return new();
        }
        public static List<RecipeTask_R> Load_0001(BinaryReader reader, Dictionary<string, Recipe> recipeMap, Dictionary<string, int> recipeGroupMap)
        {
            List<RecipeTask_R> result = new();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string checkcode = reader.ReadString();
                int state = reader.ReadInt32();
                int Count = reader.ReadInt32();
                bool stopping = reader.ReadBoolean();
                int excludeCount = reader.ReadInt32();
                Dictionary<int, Dictionary<int, bool>> exclude = new();
                for (int j = 0; j < excludeCount; j++)
                {
                    string name = reader.ReadString();
                    int ecount = reader.ReadInt32();
                    Dictionary<int, bool> sube = new();
                    for (int k = 0; k < ecount; k++)
                    {
                        string item = reader.ReadString();
                        if (!int.TryParse(item, out int type))
                        {
                            type = ModContent.TryFind(item, out ModItem modItem) ? modItem.Type : -1;
                        }
                        sube[type] = reader.ReadBoolean();
                    }
                    if (recipeGroupMap.TryGetValue(name, out int id) && !sube.ContainsKey(-1))
                    {
                        exclude[id] = sube;
                    }
                }
                if (recipeMap.TryGetValue(checkcode, out Recipe targetRecipe))
                {
                    RecipeTask_R task = new(targetRecipe)
                    {
                        TaskState = (RecipeTaskState)state,
                        Count = Count,
                        Stopping = stopping,
                        exclude = exclude
                    };
                    foreach (int trg in task.Target.acceptedGroups)
                    {
                        if (!exclude.ContainsKey(trg))
                        {
                            RecipeGroup addExcludeRG = RecipeGroup.recipeGroups[trg];
                            Dictionary<int, bool> addExclude = new();
                            foreach (int type in addExcludeRG.ValidItems)
                            {
                                addExclude[type] = false;
                            }
                            exclude[trg] = addExclude;
                        }
                    }
                    result.Add(task);
                }
            }
            return result;
        }
    }
}
