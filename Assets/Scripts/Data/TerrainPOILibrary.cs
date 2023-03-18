using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public struct TerrainPOILibrary
    {
        public Dictionary<TerrainPOI, TerrainPOIConfig>
            TerrainPOIs { get; private set; }

        public static TerrainPOILibrary EmptyLibrary()
        {

            TerrainPOILibrary result = default;

            result.TerrainPOIs = new();

            return result;
        }

        public FloatRange SpawnRateForType(TerrainPOI terrainPOI) =>
            TerrainPOIs[terrainPOI].SpawnRateInterval;

        internal void UpdateConfig(
            TerrainPOI terrainPOI,
            FloatRange range)
        {
            var config = TerrainPOIConfig.Create(
                terrainPOI, range);

            if (TerrainPOIs.TryGetValue(terrainPOI, out _))
                TerrainPOIs[terrainPOI] = config;
            else
                TerrainPOIs.Add(terrainPOI, config);
        }

    }
}