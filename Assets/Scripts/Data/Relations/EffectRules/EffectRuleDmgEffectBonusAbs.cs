namespace Assets.Scripts.Data
{
    /// <summary>
    /// Effect affects target's Damage Effect Bonus, so no need to specify Damage Effect explicitly
    /// бонус от урона +- знач (пп)	Цель		Значение	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleDmgEffectBonusAbs : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleDmgEffectBonusAbs(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            Value = rawValues[2].ParseIntValue(0, true);
            TurnsCount = rawValues[3].ParseIntValue();

            key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);        
        }

        public RelationsEffectType EffectType => RelationsEffectType.DmgEffectBonusAbs;
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion                        

        /// <summary>
        /// value will be used as an increment added to the current spec/bonus value
        /// </summary>
        public int Value { get; set; }
    }
}