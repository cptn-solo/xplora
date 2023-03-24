using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public delegate ref OpponentTeamMemberSpawnConfig TeamConfigProcessor();

    public class TeamSpawnRulesConfigLoader : BaseConfigLoader
    {
        private TeamConfigProcessor teamConfigProcessor;
        private bool process = false;

        protected override string RangeString => "'Enemy спавн рейт'!A1:F22";
        protected override string ConfigName => "EnemySpawnRules";

        public TeamSpawnRulesConfigLoader(TeamConfigProcessor teamConfigProcessor, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.teamConfigProcessor = teamConfigProcessor;
            process = true;
        }

        public TeamSpawnRulesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 7;
            if (list == null || list.Count < rowsCount || !process)
                return;

            object val(int row, int cell) => list[row][cell];

            ref var teamConfig = ref teamConfigProcessor();

            teamConfig.OveralStrengthLevels = new();

            var buffer = ListPool<int>.Get(); // quick cash to be used for strenth based imports below

            for (int row = 2; row < 6; row++)
            {
                var strength = val(row, 1).ParseIntValue();
                var spawnRate = val(row, 2).ParseIntValue();
                var r = val(row, 3).ParseIntValue();
                var g = val(row, 4).ParseIntValue();
                var b = val(row, 5).ParseIntValue();
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
                    var adjustment = val(row, 1 + i).ParseIntValue(0, true);
                    teamConfig.UpdateTeamMemberAdjustmentsByStrength(
                        buffer[i], intRange, adjustment);

                }
            }

            teamConfig.PrepareIndexes();

            ListPool<int>.Add(buffer);
        }

    }
}