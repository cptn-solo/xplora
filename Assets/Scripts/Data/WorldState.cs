namespace Assets.Scripts.Data
{
    public enum WorldState
    {
        NA = -1,
        AwaitingTerrain = 10, // Right after the navigation to the raid screen
        TerrainBeingGenerated = 200, // During the terrain generation
        SceneReady = 300, // Terrain and Units panel are initialized, cells generated
        TerrainBeingDestoyed = 350, //During the terrain destruction
    }
}