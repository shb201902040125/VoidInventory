﻿using System.Linq;
using static VoidInventory.RecipeSupport;

namespace VoidInventory.Content
{
    public class VIUI : ContainerElement
    {
        public const string NameKey = "VoidInventory.Content.VIUI";
        public bool firstLoad;
        public UIPanel bg;
        public UIBottom left, right;
        public UIContainerPanel leftView, rightView;
        public UIItemSlot focusItem;
        public UIInputBox input;
        internal static Recipe MouseRecipe;
        public int TaskCount => leftView.InnerUIE.Count;
        public UIPanel detail;
        public override void OnInitialization()
        {
            base.OnInitialization();
            if (Main.gameMenu) return;
            RemoveAll();
            bg = new(default, 800, 600);
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

            input = new(color: Color.White);
            input.SetSize(160, 36);
            input.SetPos(-input.Width - 20, 20 + 10, 1);
            input.OnInputText += () =>
            {
                FindRecipe();
            };
            input.DrawRec[0] = Color.Red;
            bg.Register(input);

            left = new(-40, -102, 0.5f, 1f);
            left.SetPos(20, 82);
            left.DrawRec[0] = Color.White;
            bg.Register(left);

            leftView = new();
            leftView.Info.SetMargin(0);
            left.Register(leftView);

            VerticalScrollbar leftscroll = new();
            leftscroll.Info.IsHidden = true;
            leftscroll.Info.Left.Pixel += 10;
            leftView.SetVerticalScrollbar(leftscroll);
            left.Register(leftscroll);

            right = new(-40, -102, 0.5f, 1f);
            right.SetPos(20, 82, 0.5f);
            right.DrawRec[0] = Color.White;
            bg.Register(right);

            rightView = new();
            rightView.Info.SetMargin(0);
            right.Register(rightView);

            VerticalScrollbar rightscroll = new();
            rightscroll.Info.IsHidden = true;
            rightscroll.Info.Left.Pixel += 10;
            rightView.SetVerticalScrollbar(rightscroll);
            right.Register(rightscroll);

            detail = new(default, 300, 500);
            detail.SetPos(0, 0, 0.1f, 0.35f);
            detail.Info.IsVisible = false;
            detail.CanDrag = true;
            Register(detail);
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
            bool have = TryFindRecipes(x => x.createItem.type == type || x.ContainsIngredient(type), out var recipes);
            string text = input.Text;
            if (text.Length > 0)
            {
                if (type == 0) TryFindRecipes(x => x.createItem.Name.Contains(text), out recipes);
                else recipes = recipes.Where(x => x.createItem.Name.Contains(text));
            }
            if (!have || !recipes.Any()) return;
            int x = 10, y = 10;
            foreach (Recipe r in recipes)
            {
                if (r.createItem.type == ItemID.None) continue;
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
        private void AddRecipeTask(Recipe recipe)
        {
            UIRecipeTask task = new(recipe)
            {
                id = TaskCount
            };
            task.SetSize(-20, 52, 1);
            task.SetPos(10, 10 + TaskCount * 62);
            leftView.AddElement(task);
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