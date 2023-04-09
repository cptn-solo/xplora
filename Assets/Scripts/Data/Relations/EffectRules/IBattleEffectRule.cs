namespace Assets.Scripts.Data
{
    public interface IBattleEffectRule : IEffectRule
    {
        /// <summary>
        /// H - rule affects subject hero's spec option
        /// E (enemy) - rule affects subject hero's current enemy
        /// </summary>
        public HeroDomain TargetDomain { get; set; }
        
        /// <summary>
        /// For how long lasts the effect (next N turns)
        /// </summary>
        public int TurnsCount { get; set; }
    }
}