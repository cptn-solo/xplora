namespace Assets.Scripts.Data
{
    public struct TerrainAttributeConfig
    {
        public TerrainType TerrainType { get; private set; }
        public TerrainAttribute Attribute { get; private set; }
        public int SpawnRate { get; private set; }

        public static TerrainAttributeConfig Create(
            TerrainType terrainType,
            TerrainAttribute attribute,
            int spawnRate)
        {
            return new()
            {
                TerrainType = terrainType,
                Attribute = attribute,
                SpawnRate = spawnRate
            };
        }
    }
}