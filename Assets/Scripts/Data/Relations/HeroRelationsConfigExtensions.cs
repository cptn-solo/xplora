using UnityEngine;

namespace Assets.Scripts.Data
{
    public static class HeroRelationsConfigExtensions
    {
        public static RelationState GetRelationState(this HeroRelationsConfig config, int score)
        {            
            if (score == 0)
                return RelationState.NA;

            var items = config.RelationStateThresholds;
            var length = items.Length;
            
            for (int i = 0; i < length; i++)
            {
                RelationStateValue item = items[i];

                if (i == 0 && score <= item.Value)
                    return item.State;

                if (i == length - 1 && score >= item.Value)
                    return item.State;

                if (item.Value > score)
                    return items[i - 1].State;
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