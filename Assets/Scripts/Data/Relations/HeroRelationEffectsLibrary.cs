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
        public Dictionary<int, int> AdditionalEffectSpawnRate { get; set; }

        public static HeroRelationEffectsLibrary EmptyLibrary()
        {
            HeroRelationEffectsLibrary result = new ()
            {
                SubjectStateEffectsIndex = new Dictionary<RelationEffectLibraryKey, HeroRelationEffectConfig>(),
                AdditionalEffectSpawnRate = new Dictionary<int, int>()
            };
            return result;
        }
    }
}