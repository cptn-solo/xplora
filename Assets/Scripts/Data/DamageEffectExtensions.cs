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
    }
}