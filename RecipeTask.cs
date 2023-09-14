using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Windows.Forms;
using Terraria.ModLoader.IO;
using VoidInventory.Content;

namespace VoidInventory
{
    public class RecipeTask
    {
        private static HashSet<Condition> ignores = new();
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
        }
        public Recipe RecipeTarget { get; private set; }
        public bool Stopping { get; internal set; }
        /// <summary>
        /// 合成任务状态
        /// <br>0:完成<see cref="CountTarget"/>次制作</br>
        /// <br>1:维持<see cref="CountTarget"/>个产物</br>
        /// <br>2:始终制作</br>
        /// </summary>
        public int TaskState = 0;
        public void SetStateWithDefault(int state)
        {
            if (state < 0 || state > 2)
            {
                return;
            }
            TaskState = state;
            CountTarget = state == 0 ? 1 : (state == 1 ? 10 : -1);
        }
        public int CountTarget;
        internal Dictionary<int, Dictionary<int, bool>> exclude = new();
        public RecipeTask(Recipe recipe, int state = 0, int count = 10)
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

        private void TryFinish_0(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = CountTarget == 0;
                return;
            }
            finishAtLeastOnce = false;
            finishAll = false;
            if (CountTarget > 0)
            {
                //合成成功，完成一次标记为true，全部完成判断次数需求是否到0
                int times = 0;
                while (DoRecipe(inv))
                {
                    times++;
                    finishAtLeastOnce = true;
                    finishAll = --CountTarget == 0;
                    if (finishAll)
                    {
                        break;
                    }
                }
                if (times > 0)
                {
                    Item item = new(RecipeTarget.createItem.type, times * RecipeTarget.createItem.stack);
                    inv.Merge(ref item);
                }
            }
        }

        private void TryFinish_1(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
        {
            int count = 0;
            if (fake)
            {
                finishAtLeastOnce = CanRecipe(inv);
                finishAll = inv._items.TryGetValue(RecipeTarget.createItem.type, out List<Item> fakeHeld) && (count = fakeHeld.Sum(i => i.stack)) >= CountTarget;
                return;
            }
            //统计背包里目标产物个数是否达到
            bool flag = inv._items.TryGetValue(RecipeTarget.createItem.type, out List<Item> held);
            if (!flag && CountTarget == 0)
            {
                finishAtLeastOnce = false;
                finishAll = true;
                return;
            }
            if (flag && (count = held.Sum(i => i.stack)) >= CountTarget)
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
                count += RecipeTarget.createItem.stack;
                finishAtLeastOnce = true;
                finishAll = count >= CountTarget;
                if (finishAll)
                {
                    break;
                }
            }
            if (times > 0)
            {
                Item item = new(RecipeTarget.createItem.type, times * RecipeTarget.createItem.stack);
                inv.Merge(ref item);
            }
        }

        private void TryFinish_2(VInventory inv, bool fake, out bool finishAtLeastOnce, out bool finishAll)
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
                Item item = new(RecipeTarget.createItem.type, times * RecipeTarget.createItem.stack);
                inv.Merge(ref item);
            }
        }

        private bool DoRecipe(VInventory inv)
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
            foreach (Item required in RecipeTarget.requiredItem)
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

        private bool CanRecipe(VInventory inv)
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
            foreach (Item required in RecipeTarget.requiredItem)
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
            foreach (int usetype in useWhat.Keys)
            {
                List<Item> list = inv._items[usetype];
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
        public int HowManyCanUse(int type, int howManyNeed, out Dictionary<int, int> useWhat, VInventory inv, Dictionary<int, int> used)
        {
            int number = 0;
            useWhat = new();
            if (inv._items.TryGetValue(type, out List<Item> held))
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
                foreach (int heldType in inv._items.Keys)
                {
                    if (heldType == type || (checkSkip && skip.TryGetValue(heldType, out bool skipThis) && skipThis))
                    {
                        continue;
                    }
                    if (group.ContainsItem(type) && group.ContainsItem(heldType))
                    {
                        number = inv._items[heldType].Sum(i => i.stack);
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

        private bool CheckTiles(Player player, VInventory inv)
        {
            foreach (int requiredTile in RecipeTarget.requiredTile)
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
        public static bool CheckTile(int tileID, Player player, VInventory inv)
        {
            return player.adjTile[tileID] || inv.CountTile(1, tileID);
        }

        private bool CheckConditions(Player player, VInventory inv)
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
            foreach (Condition condition in RecipeTarget.Conditions)
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
        internal static void Save(TagCompound tag, List<RecipeTask> tasks)
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream);
            writer.Write("0.0.0.1");
            writer.Write(tasks.Count);
            tasks.ForEach(task =>
            {
                writer.Write(task.RecipeTarget.GetCheckCode());
                writer.Write(task.TaskState);
                writer.Write(task.CountTarget);
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
        public static List<RecipeTask> Load(TagCompound tag)
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
        public static List<RecipeTask> Load_0001(BinaryReader reader, Dictionary<string, Recipe> recipeMap, Dictionary<string, int> recipeGroupMap)
        {
            List<RecipeTask> result = new();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string checkcode = reader.ReadString();
                int state = reader.ReadInt32();
                int countTarget = reader.ReadInt32();
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
                    RecipeTask task = new(targetRecipe)
                    {
                        TaskState = state,
                        CountTarget = countTarget,
                        Stopping = stopping,
                        exclude = exclude
                    };
                    foreach (int trg in task.RecipeTarget.acceptedGroups)
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
        internal string GetReportMessage()
        {
            return string.Format(GTV($"RecipeTaskFinish"), Lang.GetItemNameValue(RecipeTarget.createItem.type));
        }
        internal static void ReadFromLocal()
        {
            string file = Path.Combine(Main.SavePath, "Mods", "VoidInventory", "RTS.vif");
            if (File.Exists(file))
            {
                try
                {
                    TagCompound tag = TagIO.FromFile(file);
                    VIPlayer player = null;
                    if (!Main.LocalPlayer?.TryGetModPlayer(out player) ?? false)
                    {
                        return;
                    }
                    player.vInventory.recipeTasks.AddRange(Load(tag));
                    RTUI.RT.LoadRT();
                }
                catch (Exception ex)
                {
                    VoidInventory.Ins.Logger.Error(ex);
                }
            }
        }
        internal static void SaveToLocal()
        {
            string file = Path.Combine(Main.SavePath, "Mods", "VoidInventory");
            Directory.CreateDirectory(file);
            file = Path.Combine(file, "RTS.vif");
            VIPlayer player = null;
            if (!Main.LocalPlayer?.TryGetModPlayer(out player) ?? false)
            {
                return;
            }
            TagCompound tag = new();
            Save(tag, player.vInventory.recipeTasks);
            TagIO.ToFile(tag, file);
        }
    }
}