using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public struct HeroRelationsEffectsLibrary
    {
        public Dictionary<int, Dictionary<RelationSubjectState, HeroRelationsEffectConfig>>
            TargetStateEffectsIndex { get; set; }

        /// <summary>
        /// Probability (in %) of each additional effect spawn
        /// </summary>
        public Dictionary<int, int> AdditioinalEffectSpawnRate { get; set; }
    }
}