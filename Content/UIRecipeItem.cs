namespace VoidInventory.Content
{
    public class UIRecipeItem : BaseUIElement
    {
        public Recipe RecipeTarget { get; }
        public UIItemSlot ItemSlot { get; }
        public UIRecipeItem(Recipe recipe)
        {
            RecipeTarget = recipe;
            ItemSlot = new(recipe.createItem);
            Register(ItemSlot);
            SetSize(52, 52);
        }
    }
}
