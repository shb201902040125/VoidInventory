using System.Linq;
using Terraria.ObjectData;

namespace VoidInventory
{
    internal static class RecipeSupport
    {
        public static bool TryFindRecipe(Predicate<Recipe> predicate, out Recipe recipe)
        {
            recipe = null;
            for (int i = 0; i < Main.recipe.Length; i++)
            {
                if (Main.recipe[i] is null)
                {
                    break;
                }
                if (predicate(Main.recipe[i]))
                {
                    recipe = Main.recipe[i];
                    return true;
                }
            }
            return false;
        }
        public static bool TryFindRecipes(Predicate<Recipe> predicate, out IEnumerable<Recipe> recipes)
        {
            recipes = from Recipe r in Main.recipe where r is not null && predicate(r) select r;
            return recipes.Any();
        }
        public static Dictionary<TKey, List<Recipe>> TryFindRecipes<TKey>(IEnumerable<TKey> keys, Func<TKey, Recipe, bool> predicate)
        {
            Dictionary<TKey, List<Recipe>> result = new();
            for (int i = 0; i < Main.recipe.Length; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (recipe is null)
                {
                    break;
                }
                foreach (TKey key in keys)
                {
                    if (predicate(key, recipe))
                    {
                        if (result.TryGetValue(key, out List<Recipe> rs))
                        {
                            rs.Add(recipe);
                        }
                        else
                        {
                            result.Add(key, new List<Recipe>() { recipe });
                        }
                    }
                }
            }
            return result;
        }
        public static bool UseWood(this Recipe recipe, int invType, int reqType)
        {
            return recipe.HasRecipeGroup(RecipeGroupID.Wood) && RecipeGroup.recipeGroups[RecipeGroupID.Wood].ContainsItem(invType) && RecipeGroup.recipeGroups[RecipeGroupID.Wood].ContainsItem(reqType);
        }
        public static bool UseIronBar(this Recipe recipe, int invType, int reqType)
        {
            return recipe.HasRecipeGroup(RecipeGroupID.IronBar) && RecipeGroup.recipeGroups[RecipeGroupID.IronBar].ContainsItem(invType) && RecipeGroup.recipeGroups[RecipeGroupID.IronBar].ContainsItem(reqType);
        }
        public static bool UseSand(this Recipe recipe, int invType, int reqType)
        {
            return recipe.HasRecipeGroup(RecipeGroupID.Sand) && RecipeGroup.recipeGroups[RecipeGroupID.Sand].ContainsItem(invType) && RecipeGroup.recipeGroups[RecipeGroupID.Sand].ContainsItem(reqType);
        }
        public static bool UseFragment(this Recipe recipe, int invType, int reqType)
        {
            return recipe.HasRecipeGroup(RecipeGroupID.Fragment) && RecipeGroup.recipeGroups[RecipeGroupID.Fragment].ContainsItem(invType) && RecipeGroup.recipeGroups[RecipeGroupID.Fragment].ContainsItem(reqType);
        }
        public static bool UsePressurePlate(this Recipe recipe, int invType, int reqType)
        {
            return recipe.HasRecipeGroup(RecipeGroupID.PressurePlate) && RecipeGroup.recipeGroups[RecipeGroupID.PressurePlate].ContainsItem(invType) && RecipeGroup.recipeGroups[RecipeGroupID.PressurePlate].ContainsItem(reqType);
        }
        public static bool CheckTileRequire(this Recipe recipe)
        {
            int index = 0;
            while (index < recipe.requiredTile.Count && recipe.requiredTile[index] > 0 && recipe.requiredTile[index] < TileLoader.TileCount)
            {
                if (!Main.LocalPlayer.adjTile[recipe.requiredTile[index]])
                {
                    return false;
                }
                index++;
            }
            return true;
        }
        public static bool CheckCondition(this Recipe recipe)
        {
            if (!(!recipe.HasCondition(Condition.NearWater) || Main.LocalPlayer.adjWater &
                            !recipe.HasCondition(Condition.NearHoney) || recipe.HasCondition(Condition.NearHoney) == Main.LocalPlayer.adjHoney &
                            !recipe.HasCondition(Condition.NearLava) || recipe.HasCondition(Condition.NearLava) == Main.LocalPlayer.adjLava &
                            !recipe.HasCondition(Condition.InSnow) || Main.LocalPlayer.ZoneSnow &
                            !recipe.HasCondition(Condition.InGraveyard) || Main.LocalPlayer.ZoneGraveyard))
            {
                return false;
            }
            foreach (Condition condition in recipe.Conditions)
            {
                // RecipeAvaliable
                if (!condition.IsMet())
                {
                    return false;
                }
            }
            return true;
        }
    }
}