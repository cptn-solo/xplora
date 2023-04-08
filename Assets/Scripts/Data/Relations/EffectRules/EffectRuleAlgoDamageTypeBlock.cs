namespace Assets.Scripts.Data
{
    public struct EffectRuleAlgoDamageTypeBlock : IBattleEffectRule
    {
        public RelationsEffectType EffectType => RelationsEffectType.AlgoDamageTypeBlock;
        public string SourceString { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion

        /// <summary>
        /// Resistance/non-resistance to damage type being applied by the effect
        /// </summary>
        public DamageType DamageType { get; set; }
        
        /// <summary>
        /// 0 - set resistance to 0 (non-resistant)
        /// 1 - set resistance to 1 (resistant)
        /// </summary>
        public int Flag { get; set; }
    }
}