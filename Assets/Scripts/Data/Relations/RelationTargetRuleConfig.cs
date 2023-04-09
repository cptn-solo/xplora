using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public struct RelationTargetRuleConfig
    {
        public IntRange TiltThreshold { get; set; }
                
        public Dictionary<HeroKindGroup, RelationTargetInfo> KindGroupBonusRules { get; set; }        
    }
}