namespace Assets.Scripts.Data
{
    /// <summary>
    /// Effect affects target's Damage Effect Bonus, so no need to specify Damage Effect explicitly
    /// бонус от урона изменить до 100%/0%	Цель		Значение	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleDmgEffectBonusPercent : IBattleEffectRule
    {
        public RelationsEffectType EffectType => RelationsEffectType.DmgEffectBonusPercent;
        public string SourceString { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion                        

        /// <summary>
        /// value will be used as a new option value (untill the end of the effect)
        /// </summary>
        public int Value { get; set; }
    }
}