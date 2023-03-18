namespace Assets.Scripts.Data
{
    public struct TerrainPOIConfig
    {
        public TerrainPOI TerrainPOI { get; private set; }
        public FloatRange SpawnRateInterval {get; private set;}

        public static TerrainPOIConfig Create(
            TerrainPOI terrainPOI,
            FloatRange spawnRateInterval)
        {
            return new()
            {
                TerrainPOI = terrainPOI,
                SpawnRateInterval = spawnRateInterval,
            };
        }
    }
}