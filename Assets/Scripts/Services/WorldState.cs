namespace Assets.Scripts.Services
{
    public enum WorldState
    {
        NA = -1,
        DelegatesAttached = 100,
        TerrainBeingGenerated = 200, // During the terrain generation
        SceneReady = 300, // Terrain and Units panel are initialized, cells generated
        UnitsBeingSpawned = 400, // Player and Enemies are being spawned right now
        UnitsSpawned = 500, // Player and Enemies spawned
    }
}