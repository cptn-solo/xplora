namespace Assets.Scripts.UI.Data
{
    public struct DamageEffectInfo
    {
        public int RoundOn { get; private set; }
        public int RoundOff { get; private set; }
        public DamageEffectConfig Config { get; private set; }

        public override string ToString()
        {
            return $"{Config.Effect}:{Config.ExtraDamage}/{Config.TurnsActive}";
        }

        public DamageEffectInfo SetConfig(DamageEffectConfig config)
        {
            Config = config;

            return this;
        }

        public DamageEffectInfo SetDuration(int roundOn, int roundOff)
        {
            RoundOn = roundOn;
            RoundOff = roundOff;

            return this;
        }

        public static DamageEffectInfo Draft(DamageEffectConfig config)
        {
            DamageEffectInfo info = default;

            info = info.SetConfig(config);
            return info;
        }

    }
}