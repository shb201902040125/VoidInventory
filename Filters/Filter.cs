namespace VoidInventory.Filters
{
    public abstract class Filter<TInput, TOutput> where TOutput : IEnumerable<TInput>
    {
        public virtual string Name { get; }
        public abstract string DescriptionPath { get; }
        public abstract TOutput FilterItems(ICollection<TInput> items);
    }
}