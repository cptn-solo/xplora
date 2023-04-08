namespace Assets.Scripts.Data
{
    /// <summary>
    /// спека изменить на макс/мин	Цель	Урон	Флаг	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleSpecMaxMin : IEffectRule
    {
        public RelationsEffectType EffectType => RelationsEffectType.SpecMaxMin;
        public string SourceString { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// H - rule affects subject hero's spec option
        /// E (enemy) - rule affects subject hero's current enemy
        /// </summary>
        public HeroDomain TargetDomain { get; set; }
        public SpecOption SpecOption { get; set; }

        /// <summary>
        /// -1 => set minimum end of the ranged spec optin, 
        /// +1 => set maximium end of the ranged spec option
        /// </summary>
        public int MaxMin { get; set; }

        /// <summary>
        /// For how long lasts the effect (next N turns)
        /// </summary>
        public int TurnsCount { get; set; }

    }
}