namespace Assets.Scripts.Data
{
    public struct EffectRuleAlgoTarget : IBattleEffectRule
    {
        public EffectRuleAlgoTarget(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            TurnsCount = rawValues[2].ParseIntValue();
        }

        public RelationsEffectType EffectType => RelationsEffectType.AlgoTarget;
        public string[] Source { get; set; }
        public string Description { get; set; }

        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
    }
}