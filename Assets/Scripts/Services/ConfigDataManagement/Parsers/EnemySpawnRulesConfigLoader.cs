using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class EnemySpawnRulesConfigLoader : BaseConfigLoader
    {
        private readonly EnemySpawnRulesLibrary? library;

        protected override string RangeString => "'Enemy спавн рейт'!A1:F22";
        protected override string ConfigName => "EnemySpawnRules";

        public EnemySpawnRulesConfigLoader(EnemySpawnRulesLibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }

        public EnemySpawnRulesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 7;
            if (list == null || list.Count < rowsCount || library == null)
                return;

            object val(int row, int cell) => list[row][cell];

            var cellShare = val(0, 0).ParseIntArray();

            var opConfig = library.Value.OpponentSpawnConfig;

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

            library.Value.SetOpponentSpawnConfig(opConfig);

            var teamConfig = library.Value.OpponentTeamMemberSpawnConfig;

            teamConfig.OveralStrengthLevels = new();

            var buffer = ListPool<int>.Get(); // quick cash to be used for strenth based imports below

            for (int row = 2; row < 6; row++)
            {
                var strength = val(row, 1).ParseAbsoluteValue();
                var spawnRate = val(row, 2).ParseRateValue();
                var r = val(row, 3).ParseAbsoluteValue();
                var g = val(row, 4).ParseAbsoluteValue();
                var b = val(row, 5).ParseAbsoluteValue();
                var color = new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, 1f);

                teamConfig.UpdateTeamMemberSpawnRateByStrength(
                    strength, spawnRate, color);

                buffer.Add(strength);
            }

            for (int row = 10; row < 13; row++)
            {
                var teamStrengthRange = val(row, 0).ParseIntArray();
                var intRange = new IntRange(teamStrengthRange[0], teamStrengthRange[1]);
                for (int i = 0; i < buffer.Count; i++)
                {
                    var adjustment = val(row, 1 + i).ParseRateValue(0, true);
                    teamConfig.UpdateTeamMemberAdjustmentsByStrength(
                        buffer[i], intRange, adjustment);

                }
            }

            teamConfig.PrepareIndexes();

            ListPool<int>.Add(buffer);

            library.Value.SetOpponentTeamMemberSpawnConfig(teamConfig);
        }

    }
}