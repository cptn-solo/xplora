namespace Assets.Scripts.Data
{
   /// <summary>
   /// спека изменить +- знач (пп)	Цель	Хар-ка	Значение	длительность (до след. N хода героя)
   /// </summary>
    public struct EffectRuleSpecAbs : IBattleEffectRule
    {
        public RelationsEffectType EffectType => RelationsEffectType.SpecAbs;
        public string SourceString { get; set; }
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
        /// value will be used as an increment added to the current spec/bonus value
        /// </summary>
        public int Value { get; set; }
        
    }
}