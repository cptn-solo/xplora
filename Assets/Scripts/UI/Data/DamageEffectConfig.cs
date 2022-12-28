namespace Assets.Scripts.UI.Data
{
    public struct DamageEffectConfig
    {
        public DamageEffect Effect { get; private set; }
        public bool TurnSkipped { get; private set; }
        public int TurnsActive { get; private set; }
        public int ExtraDamage { get; private set; }
        public int ShieldUseFactor { get; private set; } // % of target's shield used to deflect extra damage

        public int ChanceRate { get; private set; }

        public static DamageEffectConfig Create(
            DamageEffect effect,
            bool turnSkipped,
            int turnsActive, 
            int extraDamage, 
            int shieldUseFactor,
            int chanceRate)
        {
            DamageEffectConfig config = default;

            config.Effect = effect;
            config.TurnSkipped = turnSkipped;
            config.TurnsActive = turnsActive;
            config.ExtraDamage = extraDamage;
            config.ShieldUseFactor = shieldUseFactor;
            config.ChanceRate = chanceRate;

            return config;
        }
    }
}