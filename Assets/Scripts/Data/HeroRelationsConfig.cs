using System.Collections.Generic;

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
        public HeroKind[] SpiritKindGroup { get; set; }
        public HeroKind[] BodyKindGroup { get; set; }
        
        public IntRange SpiritGroupScoreRange { get; set; }
        public IntRange BodyGroupScoreRange { get; set; }
        public IntRange NeutralGroupScoreRange { get; set; }

        public Dictionary<HeroKindGroup, Dictionary<HeroKindGroup, IntRange>> RelationMatrix { get; set; }
        
        public RelationStateValue[] RelationStateThresholds { get; set; }

        public Dictionary<HeroKindGroup, RelationTargetRuleConfig[]> RelationTargetRules { get; set; }
    }
}