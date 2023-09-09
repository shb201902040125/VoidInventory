using System.Linq;
using static VoidInventory.RecipeSupport;

namespace VoidInventory.Content
{
    public class RTUI : ContainerElement
    {
        public const string NameKey = "VoidInventory.Content.RTUI";
        public bool firstLoad;
        public UIPanel bg, dbg;
        public UIPanel left, right;
        public UIContainerPanel leftView, rightView;
        public UIItemSlot focusItem;
        public UIInputBox input;
        internal static Recipe MouseRecipe;
        public int TaskCount => leftView.InnerUIE.Count;
        public UIContainerPanel detail;
        public int detailID;
        public override void OnInitialization()
        {
            base.OnInitialization();
            if (Main.gameMenu)
            {
                return;
            }

            RemoveAll();
            bg = new(800, 600);
            bg.SetCenter(0, 0, 0.5f, 0.5f);
            bg.CanDrag = true;
            Register(bg);

            focusItem = new();
            focusItem.SetPos(20, 20);
            focusItem.Events.OnLeftDown += evt =>
            {
                if (Main.mouseItem.type > ItemID.None)
                {
                    ChangeItem(Main.mouseItem.type);
                }
            };
            focusItem.Events.OnRightClick += evt =>
            {
                ChangeItem(0);
            };
            bg.Register(focusItem);

            UIPanel inputbg = new(160 + 24, 30, 12, 4, Color.White, 1f);
            inputbg.SetPos(-inputbg.Width - 20, 20 + 10, 1);
            bg.Register(inputbg);

            input = new("搜索合成目标", color: Color.Black);
            input.SetSize(-24, 0, 1, 1);
            input.SetPos(12, 2);
            input.OnInputText += FindRecipe;
            inputbg.Register(input);

            UIText VI = new("背包");
            VI.SetSize(VI.TextSize);
            VI.SetCenter(0, 30, 0.5f, 0);
            VI.Events.OnLeftClick += evt =>
            {
                Info.IsVisible = false;
                ContainerElement viui = VoidInventory.Ins.uis.Elements[VIUI.NameKey];
                viui.Info.IsVisible = true;
                ((VIUI)viui).bg.SetPos(bg.Info.TotalLocation);
                viui.Calculation();
            };
            bg.Register(VI);

            left = new(0, 0);
            left.SetSize(-40, -102, 0.5f, 1);
            left.SetPos(20, 82);
            bg.Register(left);

            leftView = new();
            leftView.Info.SetMargin(0);
            left.Register(leftView);

            VerticalScrollbar leftscroll = new(62);
            //leftscroll.Info.IsHidden = true;
            leftscroll.Info.Left.Pixel += 10;
            leftView.SetVerticalScrollbar(leftscroll);
            left.Register(leftscroll);

            right = new(0, 0);
            right.SetSize(-40, -102, 0.5f, 1);
            right.SetPos(20, 82, 0.5f);
            bg.Register(right);

            rightView = new();
            rightView.Info.SetMargin(0);
            right.Register(rightView);

            VerticalScrollbar rightscroll = new(62);
            rightscroll.Info.IsHidden = true;
            rightscroll.Info.Left.Pixel += 10;
            rightView.SetVerticalScrollbar(rightscroll);
            right.Register(rightscroll);

            dbg = new(320, 500);
            dbg.SetPos(0, 0, 0.1f, 0.35f);
            dbg.Info.IsVisible = false;
            dbg.CanDrag = true;
            Register(dbg);

            detail = new();
            detail.Info.SetMargin(0);
            detail.SetSize(-10, -40, 1, 1);
            detail.SetPos(0, 20);
            dbg.Register(detail);

            VerticalScrollbar detailscroll = new(hide: true);
            detail.SetVerticalScrollbar(detailscroll);
            dbg.Register(detailscroll);
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
        public void ChangeItem(int itemType)
        {
            focusItem.ContainedItem = new(itemType);
            FindRecipe();
        }
        private void FindRecipe()
        {
            rightView.ClearAllElements();
            int type = focusItem.ContainedItem.type;
            bool have = TryFindRecipes(x => x.createItem.type == type || x.ContainsIngredient(type), out IEnumerable<Recipe> recipes);
            string text = input.Text;
            if (text.Length > 0)
            {
                if (type == 0)
                {
                    TryFindRecipes(x => x.createItem.Name.Contains(text), out recipes);
                }
                else
                {
                    recipes = recipes.Where(x => x.createItem.Name.Contains(text));
                }
            }
            if (!have || !recipes.Any())
            {
                return;
            }

            int x = 10, y = 10;
            foreach (Recipe r in recipes)
            {
                if (r.createItem.type == ItemID.None)
                {
                    continue;
                }

                UIRecipeItem recipe = new(r);
                recipe.SetPos(x, y);
                recipe.Info.IsSensitive = true;
                recipe.Events.OnLeftDown += evt =>
                {
                    MouseRecipe = recipe.RecipeTarget;
                };
                recipe.Events.OnLeftUp += evt =>
                {
                    if (leftView.ContainsPoint(Main.MouseScreen))
                    {
                        AddRecipeTask(MouseRecipe);
                    }
                    MouseRecipe = null;
                };
                recipe.Events.OnLeftDoubleClick += evt =>
                {

                };
                rightView.AddElement(recipe);
                x += 56;
                if (x + 56 > rightView.Width)
                {
                    x = 10;
                    y += 56;
                }
            }
        }
        private void AddRecipeTask(object recipe)
        {
            UIRecipeTask task;
            if (recipe is RecipeTask rt) task = new(rt);
            else if (recipe is Recipe r) task = new(r);
            else throw new Exception(recipe.GetType().Name + " is not accept");
            task.id = TaskCount;
            task.SetSize(-20, 52, 1);
            task.SetPos(10, 10 + (TaskCount * 62));
            leftView.AddElement(task);
        }
        public void LoadRT()
        {
            leftView.ClearAllElements();
            foreach (RecipeTask rt in Main.LocalPlayer.VIP().vInventory.recipeTasks)
            {
                AddRecipeTask(rt);
            }
        }
        public void SortRecipeTask(int id)
        {
            foreach (UIRecipeTask task in leftView.InnerUIE.Cast<UIRecipeTask>())
            {
                if (task.id > id)
                {
                    task.id--;
                    task.Info.Top.Pixel -= 62;
                }
            }
            leftView.Calculation();
        }
    }
}
