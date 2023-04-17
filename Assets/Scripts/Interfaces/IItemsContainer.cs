namespace Assets.Scripts
{
    public interface IItemsContainer<T> : ITransform
    {
        public void SetItemInfo(T info);
        public void SetInfo(T[] value);
        public void RemoveItem(T info);
        public void Reset();
    }

}