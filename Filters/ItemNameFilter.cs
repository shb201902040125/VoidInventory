using System.Linq;

namespace VoidInventory.Filters
{
    public class ItemNameFilter : Filter<Item, List<Item>>
    {
        internal static volatile ItemNameFilter Instance = new();
        public string NameFragment;
        public override string Name => $"{nameof(ItemNameFilter)}:{(string.IsNullOrEmpty(NameFragment) ? "EmptyString" : NameFragment)}";
        public override string DescriptionPath => $"VoidInventory.Filter.{nameof(ItemNameFilter)}";
        public override List<Item> FilterItems(ICollection<Item> items)
        {
            return string.IsNullOrEmpty(NameFragment)
                ? new()
                : (from item in items where Lang.GetItemName(item.type).Value.Contains(NameFragment) select item).ToList();
        }
    }
}