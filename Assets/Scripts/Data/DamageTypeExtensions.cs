namespace Assets.Scripts.Data
{
    public static class DamageTypeExtensions
    {
        public static BundleIcon IconCode(this DamageType damageType) =>
            damageType switch
            {
                DamageType.Burn => BundleIcon.Burning,
                DamageType.Force => BundleIcon.Stun,
                DamageType.Cut => BundleIcon.Bleeding,
                DamageType.Frost => BundleIcon.Freezing,
                DamageType.Pierce=> BundleIcon.ArmorPenetr,
                _ => BundleIcon.NA
            };
    }
}