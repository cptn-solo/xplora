using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data
{
    using SortedByStrength = IOrderedEnumerable<KeyValuePair<int, StrengthSpawnInfo>>;
    public delegate void SpawnPassCallback(int strength);

    public struct OpponentTeamMemberSpawnConfig
    {
        public Dictionary<int, StrengthSpawnInfo> OveralStrengthLevels { get; set; }

        public SortedByStrength SortedSpawnRateInfo { get; set; }

        public int RunOnePass(int teamStrength, SpawnPassCallback callback = null, int treshold = -1)
        {
            foreach (var item in SortedSpawnRateInfo)
            {
                if (item.Key > teamStrength ||
                    (treshold > 0 && item.Key > treshold))
                    continue;

                var adjustedSpawnRate = item.Value.SpawnRate;

                foreach (var adj in item.Value.TeamStrengthWeightedSpawnRates)
                {
                    if (adj.Key.Item1 < teamStrength &&
                        adj.Key.Item2 >= teamStrength)
                    {
                        adjustedSpawnRate += adj.Value;
                        break;
                    }
                }

                if (!adjustedSpawnRate.RatedRandomBool())
                    continue;

                callback?.Invoke(item.Key);

                return item.Key;
            }

            return 0;
        }
        public static OpponentTeamMemberSpawnConfig DefaultConfig()
        {
            var retval = new OpponentTeamMemberSpawnConfig()
            {
                OveralStrengthLevels = new() {
                    { 1, new () {
                        OveralStrenght = 1,
                        TintColor = new Color(77f/255f, 131f/255f, 207f/255f, 1f),
                        SpawnRate = 45,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), -20 },
                            { new (21, 30), -30 },
                        },
                    } },
                    { 2, new () {
                        OveralStrenght = 2,
                        TintColor = new Color(58f/255f, 76f/255f, 255f/255f, 1),
                        SpawnRate = 30,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), -15 },
                            { new (21, 30), -20 },
                        },
                    } },
                    { 7, new () {
                        OveralStrenght = 7,
                        TintColor = new Color(104f/255f, 38f/255f, 229f/255f, 1f),
                        SpawnRate = 15,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), 20 },
                            { new (21, 30), 20 },
                        },
                    } },
                    { 10, new () {
                        OveralStrenght = 10,
                        TintColor = new Color(226f/255f, 104f/255f, 56f/255f, 1f),
                        SpawnRate = 10,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), 15 },
                            { new (21, 30), 30 },
                        },
                    } },
                }
            };

            retval.SortedSpawnRateInfo =
                retval.OveralStrengthLevels
                .OrderByDescending(x => x.Key);

            return retval;
        }

        internal void ResetTeamMemberSpawnRates()
        {
            OveralStrengthLevels.Clear();
        }

        /// <summary>
        /// Prepares base spawn rate info to be later updated with adjustments info.
        /// Please purge existing records prior to load config
        /// </summary>
        /// <param name="strength">Hero strength</param>
        /// <param name="spawnRate">Base spawn rate (before adjustments)</param>
        /// <param name="tintColor">Tint color to be used with icons, etc</param>
        internal void UpdateTeamMemberSpawnRateByStrength(int strength, int spawnRate, Color tintColor)
        {
            if (OveralStrengthLevels.TryGetValue(strength, out var existing))
            {
                existing.OveralStrenght = strength; // redundant but still helps to organize data.
                existing.SpawnRate = spawnRate;
                existing.TintColor = tintColor;

                OveralStrengthLevels[strength] = existing;
            }
            else
            {
                var added = new StrengthSpawnInfo()
                {
                    OveralStrenght = strength,
                    SpawnRate = spawnRate,
                    TintColor = tintColor,
                    TeamStrengthWeightedSpawnRates = new(),
                };
                OveralStrengthLevels.Add(strength, added);
            }
        }

        /// <summary>
        /// Sets adjustment for hero spawn rates according to overal team strength
        /// </summary>
        /// <param name="strength">Hero strength (used as a reference)</param>
        /// <param name="teamStrength">Overal team strength range to be adjusted</param>
        /// <param name="adjustment">Adjustment value</param>
        /// <exception cref="Exception">This method can't be used untill base
        /// spawn rates are loaded in UpdateEnemyBaseSpawnRateByStrength</exception>
        internal void UpdateTeamMemberAdjustmentsByStrength(int strength, IntRange teamStrength, int adjustment)
        {
            if (!OveralStrengthLevels.TryGetValue(strength, out var existing))
                throw new Exception("No Dict for Strength Adjustment");

            var adjDict = existing.TeamStrengthWeightedSpawnRates;

            if (adjDict.TryGetValue(teamStrength, out _))
                adjDict[teamStrength] = adjustment;
            else
                adjDict.Add(teamStrength, adjustment);

            existing.TeamStrengthWeightedSpawnRates = adjDict;

            OveralStrengthLevels[strength] = existing;
        }

        internal void PrepareIndexes()
        {
            SortedSpawnRateInfo = OveralStrengthLevels
                .OrderByDescending(x => x.Key);           
        }

    }
}