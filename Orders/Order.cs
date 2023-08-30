namespace VoidInventory.Orders
{
    internal abstract class Order<T>
    {
        public abstract void OrderItems(ref List<T> items);
    }
}
