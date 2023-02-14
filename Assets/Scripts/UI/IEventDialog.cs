namespace Assets.Scripts.UI
{
    public interface IEventDialog<T> where T: struct
    {
        public void Dismiss();
        public void SetEventInfo(T info);
    }
}