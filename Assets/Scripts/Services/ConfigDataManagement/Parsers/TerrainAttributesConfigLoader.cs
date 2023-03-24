using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public class TerrainAttributesConfigLoader : BaseConfigLoader
    {
        private readonly TerrainAttributesLibrary? library;

        protected override string RangeString => "'Гексы/Черты'!A2:E8";
        protected override string ConfigName => "TerrainAttributes";

        public TerrainAttributesConfigLoader(TerrainAttributesLibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }

        public TerrainAttributesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 7;
            if (list == null || list.Count < rowsCount || library == null)
                return;

            object val(int row, int cell) => list[row][cell];

            var prevTT = TerrainType.NA;
            for (int row = 0; row < rowsCount; row++)
            {

                var terrainType = val(row, 2).ParseTerrainType();

                if (terrainType == TerrainType.NA)
                    terrainType = prevTT;
                else prevTT = terrainType;

                var terrainAttribute = val(row, 3).ParseTerrainAttribute();
                var spawnRate = val(row, 4).ParseIntValue();

                library.Value.UpdateConfig(terrainType, terrainAttribute, spawnRate);
            }
        }

    }
}