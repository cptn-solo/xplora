namespace Assets.Scripts.Data
{
    public static class DamageEffectExtensions
    {
        public static SpecOption ResistanceSpec(this DamageEffect effect) =>
            effect switch
            {
                DamageEffect.Burning => SpecOption.BurningResistanceRate,
                DamageEffect.Stunned => SpecOption.StunResistanceRate,
                DamageEffect.Bleeding => SpecOption.BleedingResistanceRate,
                DamageEffect.Frozing => SpecOption.FrozingResistanceRate,
                _ => SpecOption.NA,
            };

        public static BundleIcon IconCode(this DamageEffect effect) =>
            effect switch
            {
                DamageEffect.Burning => BundleIcon.Burning,
                DamageEffect.Stunned => BundleIcon.Stun,
                DamageEffect.Bleeding => BundleIcon.Bleeding,
                DamageEffect.Frozing => BundleIcon.Freezing,
                DamageEffect.Pierced => BundleIcon.ArmorPenetr,
                _ => BundleIcon.NA
            };
    }
}