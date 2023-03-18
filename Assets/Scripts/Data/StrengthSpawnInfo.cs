using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct StrengthSpawnInfo
    {
        public int OveralStrenght { get; set; }
        /// <summary>
        /// Color can be used to depict a visually notable attributes of a hero
        /// with a given overal strength
        /// </summary>
        public Color TintColor { get; set; }
        /// <summary>
        /// Base spawn rate for a member with a given overal strength
        /// </summary>
        public int SpawnRate { get; set; }
        /// <summary>
        /// Added (or subtructed) rate for a member spawn rate if a given member
        /// to be spawned for a team with a given total strength (ranged)
        /// </summary>
        public Dictionary<IntRange, int> TeamStrengthWeightedSpawnRates { get; set; }
    }
}