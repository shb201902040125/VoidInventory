using System.Linq;
using Terraria.Localization;
using Terraria.Map;

namespace VoidInventory.Content
{
    public class UIRecipeTask : UIRecipeItem
    {
        private static readonly Color G = new(0, 230, 100, 255);
        private static readonly Color R = new(255, 200, 50, 255);
        /// <summary>
        /// 仅用于排序
        /// </summary>
        public int id;
        public bool[] down;
        private int timer;
        public RecipeTask RT { get; private set; }
        public static readonly DynamicSpriteFont font = FontAssets.MouseText.Value;
        public UIRecipeTask(Recipe recipe) : base(recipe)
        {
            Player player = Main.LocalPlayer;
            VIPlayer vip = player.VIP();
            VInventory inv = vip.vInventory;
            Item item = recipe.createItem;
            down = new bool[4];

            RecipeTask recipeTask = new(recipe);
            inv.recipeTasks.Add(recipeTask);
            RT = recipeTask;

            float x = 57;

            UIImage state = new(TextureAssets.MagicPixel.Value, 12, 52 - 12, 0, 0, R);
            state.SetPos(52 + 5, 6);
            Register(state);

            x += 5 + state.Width;

            UIImage reduce = new(T2D("Terraria/Images/RecLeft"));
            reduce.SetPos(x, font.LineSpacing + 4);
            reduce.Events.OnLeftDown += evt => down[0] = true;
            reduce.Events.OnLeftUp += evt => down[0] = false;
            reduce.Events.OnRightDown += evt => down[1] = true;
            reduce.Events.OnRightUp += evt => down[1] = false;
            reduce.Events.OnMouseOut += evt => down[0] = down[1] = false;
            Register(reduce);

            x += 5 + reduce.Width;

            UIImage add = new(T2D("Terraria/Images/RecRight"));
            add.SetPos(x, font.LineSpacing + 4);
            add.Events.OnLeftDown += evt => down[2] = true;
            add.Events.OnLeftUp += evt => down[2] = false;
            add.Events.OnRightDown += evt => down[3] = true;
            add.Events.OnRightUp += evt => down[3] = false;
            add.Events.OnMouseOut += evt => down[2] = down[3] = false;
            Register(add);

            x += 5 + add.Width;

            UIText func = new(GTV("Func.0"), drawStyle: 0);
            func.SetSize(func.TextSize);
            func.SetPos(x, font.LineSpacing + 3);
            func.Events.OnLeftClick += evt =>
            {
                RT.Stopping = !RT.Stopping;
                state.color = RT.Stopping ? G : R;
            };
            func.Events.OnRightClick += evt =>
            {
                if (++RT.TaskState > 2)
                {
                    RT.TaskState = 0;
                }

                func.ChangeText(GTV($"Func.{RT.TaskState}"));
            };
            func.Events.OnMouseOver += evt => func.color = R;
            func.Events.OnMouseOut += evt => func.color = Color.White;
            Register(func);

            UIImage remove = new(T2D(Asset + "Close"));
            remove.SetPos(-5 - 16, 3, 1);
            remove.Events.OnLeftClick += evt =>
            {
                ParentElement.Remove(this);
                RTUI ui = VoidInventory.Ins.uis.Elements[RTUI.NameKey] as RTUI;
                ui.SortRecipeTask(id);
                inv.recipeTasks.Remove(RT);
            };
            Register(remove);

            UIText detail = new(GTV("Detail"));
            detail.SetSize(detail.TextSize);
            detail.SetPos(-detail.Width - 3, font.LineSpacing + 3, 1);
            detail.Events.OnLeftClick += evt => SetDetail(recipe);
            detail.Events.OnMouseOver += evt => detail.color = R;
            detail.Events.OnMouseOut += evt => detail.color = Color.White;
            Register(detail);
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
            if (down[0])
            {
                Change(-1, 15);
            }
            else if (down[1])
            {
                Change(-1, 4);
            }
            else if (down[2])
            {
                Change(1, 15);
            }
            else if (down[3])
            {
                Change(1, 4);
            }
            else
            {
                timer = 0;
            }
        }
        private static int RequireCount(int itemType)
        {
            int count = 0;
            Main.LocalPlayer.VIP().vInventory.HasItem(itemType, out List<Item> items);
            items.ForEach(x => count += x.stack);
            return count;
        }
        private void Change(int count, int frame)
        {
            if (RT.TaskState == 2)
            {
                return;
            }

            ref int target = ref RT.CountTarget;
            if (timer++ % frame == 0)
            {
                target = RT.TaskState == 0 ? Math.Max(0, target + count) : Math.Clamp(target + count, 0, 9999);
            }
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            Rectangle rec = HitBox();
            sb.Draw(TextureAssets.MagicPixel.Value, rec, Color.White * 0.5f);
            Vector2 pos = rec.TopLeft() + new Vector2(75, 5);
            string text = "";
            int target = RT.CountTarget;
            switch (RT.TaskState)
            {
                case 0: text += target; break;
                case 1: text = $"{RequireCount(RT.RecipeTarget.createItem.type)}/{target}"; break;
                case 2: text += RequireCount(RT.RecipeTarget.createItem.type); break;
            }
            ChatManager.DrawColorCodedStringWithShadow(sb, font, text, pos, Color.White, 0,
                Vector2.Zero, Vector2.One, -1, 1.5f);
        }
        private void SetDetail(Recipe recipe)
        {
            RTUI ui = VoidInventory.Ins.uis.Elements[RTUI.NameKey] as RTUI;
            ref bool visiable = ref ui.dbg.Info.IsVisible;

            if (!visiable)
            {
                visiable = true;
            }
            else
            {
                if (ui.detailID != id)
                {
                    visiable = true;
                    ui.detailID = id;
                }
                else
                {
                    visiable = false;
                }
            }

            if (visiable)
            {
                ui.detail.ClearAllElements();

                int x = 20, y = 0;

                UIItemSlot item = new(recipe.createItem);
                item.SetPos(x, y);
                ui.detail.AddElement(item);
                UIImage line;
                if (recipe.requiredItem.Any())
                {
                    y += item.Height + 20;

                    UIText material = new(Language.GetTextValue("LegacyTooltip.36"), drawStyle: 0);
                    material.SetSize(material.TextSize);
                    material.SetPos(x, y);
                    ui.detail.AddElement(material);

                    y += material.Height;

                    line = new(TextureAssets.MagicPixel.Value, -40, 2, 1);
                    line.SetPos(x, y - 7);
                    ui.detail.AddElement(line);

                    List<Item> requiredItems = FilterRecipeGroup(recipe, out List<(int groupID, int stack)> groups);

                    int count = 1, len = requiredItems.Count;
                    foreach (Item requiredItem in requiredItems)
                    {
                        UIItemSlot ingredient = new(requiredItem);
                        ingredient.SetPos(x, y);
                        ingredient.IgnoreOne = true;
                        ingredient.ReDraw += sb =>
                        {
                            ingredient.DrawSelf(sb);
                            DynamicSpriteFont font = FontAssets.MouseText.Value;
                            int amount = RequireCount(ingredient.ContainedItem.type);
                            string text = amount.ToString();
                            Vector2 origin = font.MeasureString(text) / 2f;
                            ChatManager.DrawColorCodedStringWithShadow(sb, font, text, ingredient.Pos()
                                + new Vector2(26, 52 + 12), amount >= requiredItem.stack ? G : R,
                                0, origin, new Vector2(0.8f), -1, 1.5f);
                        };
                        ui.detail.AddElement(ingredient);
                        if (count < len)
                        {
                            x += 52 + 4;
                            if (x + 52 > ui.detail.Width)
                            {
                                x = 20;
                                y += 52 + 4 + 28;
                            }
                        }
                        else
                        {
                            y += 52 + 4 + 28;
                        }

                        count++;
                    }

                    x = 20;
                    if (groups.Any())
                    {
                        UIText recipeGroup = new(GTV("Group"), drawStyle: 0);
                        recipeGroup.SetSize(recipeGroup.TextSize);
                        recipeGroup.SetPos(x, y);
                        ui.detail.AddElement(recipeGroup);

                        y += recipeGroup.Height + 10;

                        line = new(TextureAssets.MagicPixel.Value, -40, 2, 1);
                        line.SetPos(x, y - 17);
                        ui.detail.AddElement(line);

                        foreach ((int groupID, int stack) in groups)
                        {
                            RecipeGroup group = RecipeGroup.recipeGroups[groupID];
                            RT.exclude.TryAdd(groupID, new Dictionary<int, bool>());

                            recipeGroup = new(group.GetText.Invoke() + $" x{stack}", drawStyle: 0);
                            recipeGroup.SetSize(recipeGroup.TextSize);
                            recipeGroup.SetPos(x, y);
                            y += recipeGroup.Height;
                            ui.detail.AddElement(recipeGroup);

                            line = new(TextureAssets.MagicPixel.Value, recipeGroup.Width, 2);
                            line.SetPos(x, y - 7);
                            ui.detail.AddElement(line);
                            int gid = groupID;
                            count = 1;
                            foreach (int ValidItem in group.ValidItems)
                            {
                                UIItemSlot groupItem = new(new(ValidItem));
                                groupItem.UGI().isGroupItem = true;
                                groupItem.SetPos(x, y);
                                groupItem.IgnoreOne = true;
                                int type = ValidItem;
                                RT.exclude[gid].TryAdd(ValidItem, false);
                                groupItem.Events.OnLeftClick += uie =>
                                {
                                    bool protect = RT.exclude[gid][type];
                                    protect = !protect;
                                    RT.exclude[gid][type] = protect;
                                    groupItem.UGI().protect = protect;
                                    groupItem.SlotBackTexture = (protect ? TextureAssets.InventoryBack10
                                    : TextureAssets.InventoryBack).Value;
                                };
                                groupItem.Events.OnRightClick += uie =>
                                {
                                    bool protect = RT.exclude[gid][type];
                                    protect = !protect;
                                    protect = !protect;
                                    groupItem.UGI().protect = protect;
                                    groupItem.SlotBackTexture = (protect ? TextureAssets.InventoryBack10
                                    : TextureAssets.InventoryBack).Value;
                                };
                                groupItem.Events.OnUpdate += uie =>
                                {
                                    groupItem.ContainedItem.stack = RequireCount(groupItem.ContainedItem.type);
                                    groupItem.StackColor = groupItem.ContainedItem.stack >= stack ? G : R;
                                };
                                ui.detail.AddElement(groupItem);
                                if (count < group.ValidItems.Count)
                                {
                                    x += 56;
                                    if (x + 52 > ui.detail.Width)
                                    {
                                        x = 20;
                                        y += 52 + 4;
                                    }
                                }
                                else
                                {
                                    x = 20;
                                    y += 52 + 20;
                                }
                                count++;
                            }
                        }
                    }

                    if (recipe.requiredTile.Any())
                    {
                        x = 20;

                        UIText tiles = new(GTV("Tile"), drawStyle: 0);
                        tiles.SetSize(tiles.TextSize);
                        tiles.SetPos(x, y);
                        ui.detail.AddElement(tiles);

                        y += tiles.Height;

                        line = new(TextureAssets.MagicPixel.Value, -40, 2, 1);
                        line.SetPos(x, y - 7);
                        ui.detail.AddElement(line);

                        foreach (int tile in recipe.requiredTile)
                        {
                            int requiredTileStyle = Recipe.GetRequiredTileStyle(tile);
                            string mapObjectName = Lang.GetMapObjectName(MapHelper.TileToLookup(tile, requiredTileStyle));
                            UIText requireTile = new(mapObjectName, drawStyle: 0);
                            requireTile.SetSize(requireTile.TextSize);
                            requireTile.SetPos(x, y);
                            int tileID = tile;
                            requireTile.Events.OnUpdate += uie =>
                            {
                                requireTile.color = Main.LocalPlayer.adjTile[tileID] ? G : R;
                            };
                            ui.detail.AddElement(requireTile);
                            y += requireTile.Height;
                        }
                    }

                    if (recipe.Conditions.Any())
                    {
                        y += 10;

                        UIText cds = new(GTV("Condition"), drawStyle: 0);
                        cds.SetSize(cds.TextSize);
                        cds.SetPos(x, y);
                        ui.detail.AddElement(cds);

                        y += cds.Height;

                        line = new(TextureAssets.MagicPixel.Value, -40, 2, 1);
                        line.SetPos(x, y - 7);
                        ui.detail.AddElement(line);

                        foreach (Condition c in recipe.Conditions)
                        {
                            UIText condition = new(c.Description.Value, drawStyle: 0);
                            condition.SetSize(condition.TextSize);
                            condition.SetPos(x, y);
                            Condition cd = c;
                            condition.Events.OnUpdate += uie =>
                            {
                                condition.color = c.IsMet() ? G : R;
                            };
                            ui.detail.AddElement(condition);
                        }
                    }
                }
            }
            ui.dbg.Calculation();
        }
        private static bool IsRecipeGroup(Item item, int start, List<int> acceptedGroups, out int groupID)
        {
            groupID = -1;
            for (int i = start; i < acceptedGroups.Count; i++)
            {
                groupID = acceptedGroups[i];
                if (item.type == RecipeGroup.recipeGroups[groupID].IconicItemId)
                {
                    return true;
                }
            }
            return false;
        }
        private static List<Item> FilterRecipeGroup(Recipe recipe, out List<(int, int)> groups)
        {
            List<Item> filterRecipeGroup = new();
            groups = new();
            int index = 0;
            foreach (Item item in recipe.requiredItem)
            {
                if (IsRecipeGroup(item, index, recipe.acceptedGroups, out int groupID))
                {
                    groups.Add((groupID, item.stack));
                    index++;
                }
                else
                {
                    filterRecipeGroup.Add(item);
                }
            }
            return filterRecipeGroup;
        }
    }
}
