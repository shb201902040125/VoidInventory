using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidInventory.Orders
{
    internal class OrderItemByType : Order<Item>
    {
        public override void OrderItems(ref List<Item> items)
        {
            items= items.OrderBy(item => item.type).ToList();
        }
    }
}
