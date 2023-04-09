namespace Assets.Scripts.Data
{
    /// <summary>
    /// спека изменить на макс/мин	Цель	Урон	Флаг	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleSpecMaxMin : IBattleEffectRule
    {
        public EffectRuleSpecMaxMin(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            SpecOption = rawValues[2].ParseSpecOption();
            MaxMin = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();
         }

        public RelationsEffectType EffectType => RelationsEffectType.SpecMaxMin;
        public string[] Source { get; set; }
        public string Description { get; set; }

        public SpecOption SpecOption { get; set; }

        /// <summary>
        /// -1 => set minimum end of the ranged spec optin, 
        /// +1 => set maximium end of the ranged spec option
        /// </summary>
        public int MaxMin { get; set; }

        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion

    }
}