namespace Assets.Scripts.Data
{
    public struct EffectRuleAlgoDamageTypeBlock : IBattleEffectRule
    {
        public EffectRuleAlgoDamageTypeBlock(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            DamageType = rawValues[2].ParseDamageType();
            Flag = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();
        }

        public RelationsEffectType EffectType => RelationsEffectType.AlgoDamageTypeBlock;
        public string[] Source { get; set; }
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