using System.Security.Cryptography;
using System.Text;

namespace VoidInventory.Reverse
{
    internal static class Utils_R
    {
        private class ItemNode
        {
            public int type;
            public HashSet<ItemNode> Materials = new();
            public HashSet<ItemNode> Products = new();
            public HashSet<Recipe> AsMaterial = new();
            public HashSet<Recipe> AsProduct = new();
        }

        private static Dictionary<int, ItemNode> _nodes = new();
        private static MD5 md5 = MD5.Create();

        private static ItemNode GetOrCreateItemNode(int type)
        {
            if (_nodes.TryGetValue(type, out ItemNode node))
            {
                return node;
            }
            _nodes[type] = new() { type = type };
            return _nodes[type];
        }
        public static void PrepareItemNodeMap()
        {
            if (_nodes.Count > 0)
            {
                return;
            }
            foreach (Recipe recipe in Main.recipe[0..Recipe.numRecipes])
            {
                ItemNode create = GetOrCreateItemNode(recipe.createItem.type);
                create.AsProduct.Add(recipe);
                foreach (Item item in recipe.requiredItem)
                {
                    ItemNode node = GetOrCreateItemNode(item.type);
                    node.Products.Add(create);
                    node.AsMaterial.Add(recipe);
                    create.Materials.Add(node);
                }
            }
        }
        public static bool TryGetRecipeAsMaterial(int type, out List<Recipe> recipes)
        {
            recipes = new();
            PrepareItemNodeMap();
            if (!_nodes.TryGetValue(type, out ItemNode node))
            {
                return false;
            }
            recipes.AddRange(node.AsMaterial);
            return true;
        }
        public static bool TryGetRecipeAsProduct(int type, out List<Recipe> recipes)
        {
            recipes = new();
            PrepareItemNodeMap();
            if (!_nodes.TryGetValue(type, out ItemNode node))
            {
                return false;
            }
            recipes.AddRange(node.AsProduct);
            return true;
        }
        public static string GetCheckCode(this Recipe recipe)
        {
            StringBuilder sb = new();
            void Write(Item item)
            {
                sb.Append(item.ModItem?.FullName ?? (item.type.ToString()));
                sb.Append(item.stack);
            }
            Write(recipe.createItem);
            recipe.requiredItem.ForEach(Write);
            recipe.requiredTile.ForEach(tile => sb.Append(tile));
            recipe.Conditions.ForEach(c => sb.Append(c.Description.Key));
            recipe.acceptedGroups.ForEach(id => sb.Append(RecipeGroup.recipeGroups[id].GetText()));
            return Encoding.UTF8.GetString(md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}
