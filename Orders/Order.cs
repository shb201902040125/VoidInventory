using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidInventory.Orders
{
    internal abstract class Order<TInput,TBy> where TBy : IEquatable<TBy>
    {
    }
}
