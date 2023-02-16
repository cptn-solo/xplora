using Assets.Scripts.Data;

namespace Assets.Scripts.World
{
    public interface IVisibility
    {
        public void IncreaseVisibility();
        public void DecreaseVisibility();
        public void ResetVisibility();
        public void Load(TerrainType terrainType, bool explored);

    }
}