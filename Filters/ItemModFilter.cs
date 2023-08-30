using System.Linq;

namespace VoidInventory.Filters
{
    public class ItemModFilter : Filter<Item, List<Item>>
    {
        internal static volatile ItemModFilter Instance = new();
        public Mod Mod;
        public override string Name => $"{nameof(ItemModFilter)}:{Mod?.Name ?? "Null"}";
        public override string DescriptionPath => $"VoidInventory.Filter.{nameof(ItemModFilter)}";
        public override List<Item> FilterItems(ICollection<Item> items)
        {
            return Mod is null || !ModLoader.Mods.Contains(Mod)
                ? new()
                : (from item in items where item.ModItem is not null && item.ModItem.Mod == Mod select item).ToList();
        }
    }
}
