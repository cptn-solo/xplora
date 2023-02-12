namespace Assets.Scripts.World
{
    public interface IVisibility
    {
        public void IncreaseVisibility();
        public void DecreaseVisibility();
        public void ResetVisibility();
        public void Load(int tti, bool explored);

    }
}