using Assets.Scripts.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public delegate ref OpponentSpawnConfig OpponentConfigProcessor();

    public class EnemySpawnRulesConfigLoader : BaseConfigLoader
    {
        private readonly OpponentConfigProcessor opConfigProcessor;
        private bool process = false;

        protected override string RangeString => "'Enemy спавн рейт'!A1:F22";
        protected override string ConfigName => "EnemySpawnRules";

        public EnemySpawnRulesConfigLoader(
            OpponentConfigProcessor opConfigProcessor, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.opConfigProcessor = opConfigProcessor;
            process = true;
        }

        public EnemySpawnRulesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 7;

            if (list == null || list.Count < rowsCount || !process)
                return;

            object val(int row, int cell) => list[row][cell];

            var cellShare = val(0, 0).ParseIntArray();

            ref var opConfig = ref opConfigProcessor();

            opConfig.SpawnRateForWorldChunk = new IntRange(cellShare[0], cellShare[1]);

            opConfig.SpawnRatesForTeamStrength = new();

            for (int row = 17; row < 22; row++)
            {
                var teamStrengthRange = val(row, 0).ParseIntArray();
                var spawnRate = val(row, 1).ParseRateValue();

                opConfig.UpdateEnemySpawnRateForStrength(
                    spawnRate,
                    new IntRange(teamStrengthRange[0], teamStrengthRange[1]));
            }            
        }
    }
}