using System.Linq;

namespace VoidInventory.Orders
{
    internal class OrderItemByType : Order<Item>
    {
        public override void OrderItems(ref List<Item> items)
        {
            items = items.OrderBy(item => item.type).ToList();
        }
    }
}
