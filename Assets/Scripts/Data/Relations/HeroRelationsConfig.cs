using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct RelationBonusInfo
    {
        public int Bonus { get; set; }
        public HeroKind[] TargetKinds { get; set; }

        public static RelationBonusInfo Empty
        {
            get =>
            new()
            { Bonus = 0, TargetKinds = new HeroKind[0] };
        }
    }

    public struct RelationTargetInfo
    {
        public int Rate { get; set; }
        public int Compared { get; set; } // -1 less, 0 equals, 1 above
        public RelationBonusInfo IncomingBonus { get; set; }
        public RelationBonusInfo OutgoingBonus { get; set; }
    }

    public struct RelationTargetRuleConfig
    {
        public IntRange TiltThreshold { get; set; }
                
        public Dictionary<HeroKindGroup, RelationTargetInfo> KindGroupBonusRules { get; set; }        
    }

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

    public static class HeroRelationsConfigExtensions
    {
        public static RelationState GetRelationState(this HeroRelationsConfig config, int score)
        {
            foreach (var item in config.RelationStateThresholds)
            {
                if (item.Value > score)
                    return item.State;
            }

            return RelationState.NA;
        }

        public static HeroKindGroup GetKindGroup(this HeroRelationsConfig config, int rsd)
        {
            
            HeroKindGroup retval = 
                config.SpiritGroupScoreRange.Contains(rsd) ? HeroKindGroup.Spirit :
                config.BodyGroupScoreRange.Contains(rsd) ? HeroKindGroup.Body :
                config.NeutralGroupScoreRange.Contains(rsd) ? HeroKindGroup.Neutral :
                HeroKindGroup.NA;
            return retval;
        }

        public static RelationTargetRuleConfig? GetTargetRules(this HeroRelationsConfig config, 
            int rsd, HeroKindGroup? kindGroup = null) { 
            
            kindGroup ??= GetKindGroup(config, rsd);

            if (kindGroup == HeroKindGroup.NA)
                return null;

            var groupRules = config.RelationTargetRules[kindGroup.Value];
            var rdsAbs = Mathf.Abs(rsd);

            RelationTargetRuleConfig? retval = null;

            foreach (var item in groupRules)
            {
                if (!item.TiltThreshold.Contains(rdsAbs)) continue;

                retval = item;
                
                break;
            }

            return retval;
        }

        public static float TriggerSpawnRate(this HeroRelationsConfig config, int rsd)
        {
            var retval = 0f;
            foreach (var item in config.EventSpawnRateThresholds)
            {
                if (!item.RSDRange.Contains(rsd)) continue;

                retval = item.RateBasis + item.RateFactor * (rsd - item.RateBasis);
                
                break;
            }

            return retval;
        }
    }
}