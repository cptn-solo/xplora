using System.Collections.Generic;

namespace Assets.Scripts.Data
{

    public struct HeroRelationEffectsLibrary
    {
        public Dictionary<RelationEffectLibraryKey, HeroRelationEffectConfig>
            SubjectStateEffectsIndex { get; set; }

        /// <summary>
        /// Probability (in %) of each additional effect spawn
        /// </summary>
        public Dictionary<int, int> AdditioinalEffectSpawnRate { get; set; }

        public static HeroRelationEffectsLibrary EmptyLibrary()
        {
            HeroRelationEffectsLibrary result = new HeroRelationEffectsLibrary()
            {
                SubjectStateEffectsIndex = new Dictionary<RelationEffectLibraryKey, HeroRelationEffectConfig>(),
                AdditioinalEffectSpawnRate = new Dictionary<int, int>()
            };
            return result;
        }
    }
}