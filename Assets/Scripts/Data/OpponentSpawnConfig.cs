using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    using Random = UnityEngine.Random;
    using RangedRate = Tuple<int, int>;

    public struct OpponentSpawnConfig
    {
        /// <summary>
        /// Defines a percentage of world cells available for
        /// opponent teams to be spawned at
        /// 
        /// </summary>
        public RangedRate SpawnRateForWorldChunk { get; set; }

        /// <summary>
        /// Defines a spawn rate for opponent teams with a given total team strength
        /// (range)
        /// </summary>
        public Dictionary<int, RangedRate> SpawnRatesForTeamStrength { get; set; }

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
                    return Random.Range(strengthRange.Item1, strengthRange.Item2 + 1);
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

    }
}