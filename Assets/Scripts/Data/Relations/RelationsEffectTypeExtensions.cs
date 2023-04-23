namespace Assets.Scripts.Data
{
    public static class RelationsEffectTypeExtensions
    {
        public static RelationsEffectClass EffectClass(this RelationsEffectType effectType)
            => effectType switch
            {
                RelationsEffectType.NA => RelationsEffectClass.NA,
                _ => RelationsEffectClass.Battle
            };

        public static BundleIcon EffectIcon(this RelationsEffectType effectType)
            => effectType switch
            {
                RelationsEffectType.NA => BundleIcon.NA,
                RelationsEffectType.SpecMaxMin => BundleIcon.Power,
                RelationsEffectType.SpecAbs => BundleIcon.Power,
                RelationsEffectType.SpecPercent => BundleIcon.Power,
                RelationsEffectType.DmgEffectAbs => BundleIcon.Sword,
                RelationsEffectType.DmgEffectPercent => BundleIcon.Sword,
                RelationsEffectType.DmgEffectBonusAbs => BundleIcon.Flame,
                RelationsEffectType.DmgEffectBonusPercent => BundleIcon.Flame,
                RelationsEffectType.AlgoRevenge => BundleIcon.Sword,
                RelationsEffectType.AlgoTarget => BundleIcon.Sword,
                RelationsEffectType.AlgoDamageTypeBlock => BundleIcon.ShieldCrossed,
                _ => BundleIcon.SnowFlake,
            };
    }
}