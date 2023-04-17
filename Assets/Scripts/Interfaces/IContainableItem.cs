namespace Assets.Scripts
{
    public interface IContainableItem<T> where T : struct
    {
        public void SetInfo(T info);
    }

}