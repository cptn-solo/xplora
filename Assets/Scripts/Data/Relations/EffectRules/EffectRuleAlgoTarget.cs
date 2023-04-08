namespace Assets.Scripts.Data
{
    public struct EffectRuleAlgoTarget : IBattleEffectRule
    {
        public RelationsEffectType EffectType => RelationsEffectType.AlgoTarget;
        public string SourceString { get; set; }
        public string Description { get; set; }

        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
    }
}