namespace Assets.Scripts
{
    public interface IDataView<T> : ITransform
    {
        public void SetInfo(T info);
        public void Reset();
    }

}