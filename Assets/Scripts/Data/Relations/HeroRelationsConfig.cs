using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public struct HeroRelationsConfig
    {
        /// <summary>
        /// These kinds are related to Spirit
        /// </summary>
        public HeroKind[] SpiritKindGroup { get; set; }

        /// <summary>
        /// These kinds are related to Body
        /// </summary>
        public HeroKind[] BodyKindGroup { get; set; }
        
        /// <summary>
        /// Hero is treated as a Spirit dominated one if his score is within the range
        /// </summary>
        public IntRange SpiritGroupScoreRange { get; set; }

        /// <summary>
        /// Hero is treated as a Body dominated one if his score is within the range
        /// </summary>
        public IntRange BodyGroupScoreRange { get; set; }

        /// <summary>
        /// Hero is treated as a Neutral one if his score is within the range
        /// </summary>
        public IntRange NeutralGroupScoreRange { get; set; }

        /// <summary>
        /// Defines how much heroes affect each other during each event spawn round (when each heroes are being
        /// probed by another ones to produce relative score difference - RSD).
        /// The more RSD is in a pair of two given heroes the more chance this pair will get an event spawned
        /// between them.
        /// </summary>
        public Dictionary<HeroKindGroup, Dictionary<HeroKindGroup, IntRange>> RelationMatrix { get; set; }
        
        /// <summary>
        /// Relative score difference will have this event spawn probability rate if within the range
        /// </summary>
        public RelationEventTriggerRange[] EventSpawnRateThresholds { get; set; }

        /// <summary>
        /// This is a so called Score Scale to define a relation state between two given heroes
        /// </summary>
        public RelationStateValue[] RelationStateThresholds { get; set; }

        /// <summary>
        /// Rules to define how each event will affect its parties in terms of individual hero kinds
        /// </summary>
        public Dictionary<HeroKindGroup, RelationTargetRuleConfig[]> RelationTargetRules { get; set; }
    }
}