using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Data
{
    public struct TerrainAttributesLibrary
    {
        public Dictionary<TerrainAttribute, TerrainAttributeConfig>
            TerrainAttributes { get; private set; }

        public static TerrainAttributesLibrary EmptyLibrary()
        {

            TerrainAttributesLibrary result = default;

            result.TerrainAttributes = new();

            return result;
        }

        internal int SpawnRateForAttribute(TerrainAttribute attribute)
        {
            if (TerrainAttributes.TryGetValue(attribute, out var config))
                return config.SpawnRate;

            return 0;
        }

        internal Dictionary<TerrainAttribute, TerrainAttributeConfig>
            AttributesForTerrainType(TerrainType terrainType)
        {
            return TerrainAttributes
                .Where(x => x.Value.TerrainType == terrainType)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        internal void UpdateConfig(
            TerrainType terrainType,
            TerrainAttribute attribute,
            int spawnRate)
        {
            var config = TerrainAttributeConfig.Create(
                terrainType, attribute, spawnRate);

            if (TerrainAttributes.TryGetValue(attribute, out _))
                TerrainAttributes[attribute] = config;
            else
                TerrainAttributes.Add(attribute, config);
        }
    }
}