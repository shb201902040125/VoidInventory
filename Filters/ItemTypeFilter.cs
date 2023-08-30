using System.Linq;

namespace VoidInventory.Filters
{
    public class ItemTypeFilter : Filter<Item, List<Item>>
    {
        internal static volatile ItemTypeFilter Instance = new();
        public int Type;
        public override string Name => $"{nameof(ItemTypeFilter)}:{Type}";
        public override string DescriptionPath => $"VoidInventory.Filter.{nameof(ItemTypeFilter)}";
        public override List<Item> FilterItems(ICollection<Item> items)
        {
            return (from item in items where item.type == Type select item).ToList();
        }
    }
}