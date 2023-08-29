using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (string.IsNullOrEmpty(NameFragment))
            {
                return new();
            }
            return (from item in items where Lang.GetItemName(item.type).Value.Contains(NameFragment) select item).ToList();
        }
    }
}