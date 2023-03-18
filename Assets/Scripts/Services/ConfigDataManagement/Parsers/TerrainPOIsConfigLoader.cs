using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class TerrainPOIsConfigLoader : BaseConfigLoader
    {
        private readonly TerrainPOILibrary? library;

        protected override string RangeString => "'POI спавн рейт'!A2:C4";
        protected override string ConfigName => "TerrainPOIs";

        public TerrainPOIsConfigLoader(TerrainPOILibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }

        public TerrainPOIsConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 3;
            if (list == null || list.Count < rowsCount || library == null)
                return;

            object val(int row, int cell) => list[row][cell];

            for (int row = 0; row < rowsCount; row++)
            {

                var terrainPOI = val(row, 0).ParseTerrainPOI();
                var spawnRateMin = val(row, 1).ParseFloatRateValue();
                var spawnRateMax = val(row, 2).ParseFloatRateValue();

                library.Value.UpdateConfig(terrainPOI, new FloatRange(spawnRateMin, spawnRateMax));
            }
        }

    }
}