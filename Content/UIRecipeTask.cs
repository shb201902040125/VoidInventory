using Terraria.Localization;
using Terraria.Map;

namespace VoidInventory.Content
{
    public class UIRecipeTask : UIRecipeItem
    {
        public int id;
        public int type;
        private readonly int[] time;
        private readonly bool[] down;
        private int timer;
        public bool enable;
        private static readonly Color G = new(0, 230, 100, 255);
        private static readonly Color R = new(255, 200, 50, 255);
        public int Lave
        {
            get { return time[0]; }
            set { time[0] = value; }
        }
        public int Has
        {
            get { return time[1]; }
            set { time[1] = value; }
        }
        public int Max
        {
            get { return time[2]; }
            set { time[2] = value; }
        }
        public DynamicSpriteFont font;
        public UIRecipeTask(Recipe recipe) : base(recipe)
        {
            Item item = recipe.createItem;
            time = new int[3] { 0, 0, 10 };
            font = FontAssets.MouseText.Value;
            down = new bool[4];

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
                enable = !enable;
                state.color = enable ? G : R;
            };
            func.Events.OnRightClick += evt =>
            {
                if (++type > 2) type = 0;
                func.ChangeText(GTV($"Func.{type}"));
            };
            func.Events.OnMouseOver += evt => func.color = R;
            func.Events.OnMouseOut += evt => func.color = Color.White;
            Register(func);

            UIImage remove = new(T2D(Asset + "Close"));
            remove.SetPos(-5 - 16, 3, 1);
            remove.Events.OnLeftClick += evt =>
            {
                ParentElement.Remove(this);
                VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
                ui.SortRecipeTask(id);
            };
            Register(remove);

            UIText detail = new(GTV("Detail"));
            detail.SetSize(detail.TextSize);
            detail.SetPos(-detail.Width - 3, font.LineSpacing + 3, 1);
            detail.Events.OnLeftClick += evt =>
            {
                VIUI ui = VoidInventory.Ins.uis.Elements[VIUI.NameKey] as VIUI;
                ui.detail.Info.IsVisible = !ui.detail.Info.IsVisible;
                ui.detail.RemoveAll();

                if (ui.detail.Info.IsVisible)
                {

                    int x = 20, y = 20;

                    UIItemSlot item = new(recipe.createItem);
                    item.SetPos(x, y);
                    ui.detail.Register(item);

                    y += item.Height + 20;

                    UIText material = new(Language.GetTextValue("LegacyTooltip.36"), drawStyle: 0);
                    material.SetSize(material.TextSize);
                    material.SetPos(x, y);
                    ui.detail.Register(material);

                    y += material.Height;

                    List<(int groupID, int stack)> groups = new();
                    foreach (Item requiredItem in recipe.requiredItem)
                    {
                        bool isGroupItem = false;
                        foreach (int acceptedGroups in recipe.acceptedGroups)
                        {
                            bool cotains = false;
                            RecipeGroup group = RecipeGroup.recipeGroups[acceptedGroups];
                            foreach (int ValidItem in group.ValidItems)
                            {
                                if (requiredItem.type == ValidItem)
                                {
                                    cotains = true;
                                    groups.Add((acceptedGroups, requiredItem.stack));
                                    break;
                                }
                            }
                            if (cotains)
                            {
                                isGroupItem = true;
                                break;
                            }
                        }
                        if (isGroupItem)
                        {
                            continue;
                        }
                        UIItemSlot ingredient = new(requiredItem);
                        ingredient.SetPos(x, y);
                        ui.detail.Register(ingredient);
                        x += 52 + 4;
                        if (x + 52 > ui.detail.Width)
                        {
                            x = 20;
                            y += 4;
                        }
                    }
                    x = 20;
                    y += 52 + 16;
                    foreach (var (groupID, stack) in groups)
                    {
                        RecipeGroup group = Terraria.RecipeGroup.recipeGroups[groupID];
                        UIText RecipeGroup = new(GTV("Group") + " " + group.GetText.Invoke(), drawStyle: 0);
                        RecipeGroup.SetSize(RecipeGroup.TextSize);
                        RecipeGroup.SetPos(x, y);
                        y += RecipeGroup.Height;
                        ui.detail.Register(RecipeGroup);
                        foreach (int ValidItem in group.ValidItems)
                        {
                            UIItemSlot groupItem = new(new(ValidItem, stack));
                            groupItem.SetPos(x, y);
                            ui.detail.Register(groupItem);
                            x += 56;
                            if (x + 52 > ui.detail.Width)
                            {
                                x = 20;
                                y += 4;
                            }
                        }
                    }
                    x = 20;
                    y += 52 + 16;

                    UIText tiles = new(GTV("Tile"), drawStyle: 0);
                    tiles.SetSize(tiles.TextSize);
                    tiles.SetPos(x, y);
                    ui.detail.Register(tiles);

                    y += tiles.Height;

                    foreach (int tile in recipe.requiredTile)
                    {
                        int requiredTileStyle = Recipe.GetRequiredTileStyle(tile);
                        string mapObjectName = Lang.GetMapObjectName(MapHelper.TileToLookup(tile, requiredTileStyle));
                        UIText requireTile = new(mapObjectName, drawStyle: 0);
                        requireTile.SetSize(tiles.TextSize);
                        requireTile.SetPos(x, y);
                        ui.detail.Register(requireTile);
                        y += requireTile.Height;
                    }

                    y += 10;

                    UIText cds = new(GTV("Condition"), drawStyle: 0);
                    cds.SetSize(cds.TextSize);
                    cds.SetPos(x, y);
                    ui.detail.Register(cds);

                    y += cds.Height;

                    foreach (Condition c in recipe.Conditions)
                    {
                        UIText condition = new(c.Description.Value, drawStyle: 0);
                        condition.SetSize(condition.TextSize);
                        tiles.SetPos(x, y);
                        ui.detail.Register(condition);
                    }
                }
            };
            detail.Events.OnMouseOver += evt => detail.color = R;
            detail.Events.OnMouseOut += evt => detail.color = Color.White;
            Register(detail);
        }
        private void Change(int count, int frame)
        {
            if (type == 2) return;
            if (timer++ % frame == 0)
            {
                if (type == 0) Lave = Math.Max(0, Lave + count);
                else Max = Math.Clamp(Max + count, 0, 9999);
            }
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
            if (down[0]) Change(-1, 15);
            else if (down[1]) Change(-1, 4);
            else if (down[2]) Change(1, 15);
            else if (down[3]) Change(1, 4);
            else timer = 0;
        }
        public override void DrawSelf(SpriteBatch sb)
        {
            Rectangle rec = HitBox();
            sb.Draw(TextureAssets.MagicPixel.Value, rec, Color.White * 0.5f);
            Vector2 pos = rec.TopLeft() + new Vector2(75, 5);
            string text = "";
            switch (type)
            {
                case 0: text += Lave; break;
                case 1: text = $"{Has}/{Max}"; break;
                case 2: text += Has; break;
            }
            ChatManager.DrawColorCodedStringWithShadow(sb, font, text, pos, Color.White, 0,
                Vector2.Zero, Vector2.One, -1, 1.5f);
        }
    }
}
