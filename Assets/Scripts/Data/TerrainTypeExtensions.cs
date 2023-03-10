using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public static class TerrainTypeExtensions
    {
        public static IEnumerable<TerrainType> TerraintTypes =
            (IEnumerable<TerrainType>)typeof(TerrainType).GetEnumValues();

        /// <summary>
        /// Will pick a ranged value based on a passed random int 0 - 100
        /// </summary>
        /// <param name="idx">1 - 100</param>
        /// <returns></returns>
        public static TerrainType RandomRangedTerrainType(this int idx)
        {
            var total = 0;
            foreach (TerrainType item in TerraintTypes)
            {
                var rate = item.SpawnRate();

                if (idx > total && idx <= total + rate)
                    return item;

                total += rate;
            }
            return TerrainType.NA;
        }

        public static int SpawnRate(this TerrainType terrainType) =>
            terrainType switch
            {
                TerrainType.NoGo => 20,
                TerrainType.Grass => 40,
                TerrainType.LightGrass => 40,
                _ => 0
            };
    }
}