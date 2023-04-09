namespace Assets.Scripts.Data
{
    /// <summary>
    /// эффект от урона изменить до 100%/0%	Цель	Эффект	Значение	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleDmgEffectPercent : IBattleEffectRule
    {
        public EffectRuleDmgEffectPercent(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            DamageEffect = rawValues[2].ParseDamageEffect();
            Value = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();
        }

        public RelationsEffectType EffectType => RelationsEffectType.DmgEffectPercent;
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
                
        /// <summary>
        /// Damage Effect will be boosted using the provided Value
        /// </summary>
        public DamageEffect DamageEffect { get; set; }  

        /// <summary>
        /// value will be used as a new option value (untill the end of the effect)
        /// </summary>
        public int Value { get; set; }
    }
}