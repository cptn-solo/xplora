namespace Assets.Scripts.Data
{

    /// <summary>
    /// спека изменить до 100%/0%	Цель	Хар-ка	Значение	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleSpecPercent : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleSpecPercent(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            SpecOption = rawValues[2].ParseSpecOption();
            Value = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();

            key = new RelationEffectKey(SpecOption, DamageEffect.NA, DamageType.NA, RelationsEffectType.NA);
        }

        public RelationsEffectType EffectType => RelationsEffectType.SpecPercent;
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
                
        /// <summary>
        /// Spec Option will be boosted using the provided Value
        /// </summary>
        public SpecOption SpecOption { get; set; }  

        /// <summary>
        /// value will be used as a new option value (untill the end of the effect)
        /// </summary>
        public int Value { get; set; }
    }
}