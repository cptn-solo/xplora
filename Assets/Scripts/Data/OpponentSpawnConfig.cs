using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    using Random = UnityEngine.Random;

    public struct OpponentSpawnConfig
    {
        /// <summary>
        /// Defines a percentage of world cells available for
        /// opponent teams to be spawned at
        /// 
        /// </summary>
        public IntRange SpawnRateForWorldChunk { get; set; }

        /// <summary>
        /// Defines a spawn rate for opponent teams with a given total team strength
        /// (range)
        /// </summary>
        public Dictionary<int, IntRange> SpawnRatesForTeamStrength { get; set; }

        /// <summary>
        /// Returns random strength for a team wheighted on a spawn rate
        /// for teams with ranged strength
        /// </summary>
        /// <param name="idx">1 - 100</param>
        /// <returns></returns>
        public int RandomRangedTeamStrength(int idx)
        {
            var total = 0;
            foreach (var rate in SpawnRatesForTeamStrength.Keys)
            {
                if (idx > total && idx <= total + rate)
                {
                    var strengthRange = SpawnRatesForTeamStrength[rate];
                    return Random.Range(strengthRange.MinRate, strengthRange.MaxRate + 1);
                }

                total += rate;
            }
            return 0;
        }

        public static OpponentSpawnConfig DefaultConfig =>
            new()
            {
                SpawnRateForWorldChunk = new(1, 2),
                SpawnRatesForTeamStrength = new() {
                    { 30, new (1, 5) },
                    { 40, new (6, 10) },
                    { 15, new (11, 15) },
                    { 10, new (16, 20) },
                    { 5, new (21, 30) },
                },
            };

        /// <summary>
        /// % of the world's available cells occupied by enemies
        /// </summary>
        /// <param name="intRange">minimut to maximum (will be randomly picked from this
        /// range by the raid generation process)</param>
        internal void UpdateEnemyCellsShare(IntRange intRange)
        {
            SpawnRateForWorldChunk = intRange;
        }

        /// <summary>
        /// Reset dictionary before update to prevent dummy entries not linked to
        /// the config file
        /// </summary>
        internal void ResetEnemySpawnRateForStrength()
        {
            SpawnRatesForTeamStrength.Clear();
        }

        /// <summary>
        /// Spawn rate for a team strength range. Old records from
        /// OpponentSpawnConfig.SpawnRatesForTeamStrength must be purged
        /// before this method is called for the 1st time in config loading process
        /// </summary>
        /// <param name="spawnRate">spawn rate</param>
        /// <param name="teamStrength">total team strength range</param>
        internal void UpdateEnemySpawnRateForStrength(int spawnRate, IntRange teamStrength)
        {
            if (SpawnRatesForTeamStrength.TryGetValue(spawnRate, out _))
                SpawnRatesForTeamStrength[spawnRate] = teamStrength;
            else
                SpawnRatesForTeamStrength.Add(spawnRate, teamStrength);
        }

    }
}