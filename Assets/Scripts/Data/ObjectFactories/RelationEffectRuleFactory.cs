namespace Assets.Scripts.Data
{
    public static class RelationEffectRuleFactory
    {
        public static IEffectRule CreateWithStringParams(string[] rawValues)
        {
            RelationsEffectType effectType = rawValues[0].ParseRelationEffectType();

                if (effectType == RelationsEffectType.NA)
                    return null;

            return effectType switch
            {
                RelationsEffectType.SpecMaxMin => new EffectRuleSpecMaxMin(rawValues),
                RelationsEffectType.SpecAbs => new EffectRuleSpecAbs(rawValues),
                RelationsEffectType.SpecPercent => new EffectRuleSpecPercent(rawValues),
                RelationsEffectType.DmgEffectAbs => new EffectRuleDmgEffectAbs(rawValues),
                RelationsEffectType.DmgEffectPercent => new EffectRuleDmgEffectPercent(rawValues),
                RelationsEffectType.DmgEffectBonusAbs => new EffectRuleDmgEffectBonusAbs(rawValues),
                RelationsEffectType.DmgEffectBonusPercent => new EffectRuleDmgEffectBonusPercent(rawValues),
                RelationsEffectType.AlgoRevenge => new EffectRuleAlgoRevenge(rawValues),
                RelationsEffectType.AlgoTarget => new EffectRuleAlgoTarget(rawValues),
                RelationsEffectType.AlgoDamageTypeBlock => new EffectRuleAlgoDamageTypeBlock(rawValues),
                _ => null,
            };
        }
    }
}