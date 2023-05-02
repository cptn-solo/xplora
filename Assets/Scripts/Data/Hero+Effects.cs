namespace Assets.Scripts.Data
{
    public partial struct Hero // Effects
    {
        // casted effect resistance probability
        public bool RandomResistStun => ResistStunRate.RatedRandomBool();
        public bool RandomResistBleeding => ResistBleedRate.RatedRandomBool();
        public bool RandomResistPierced => false;
        public bool RandomResistBurning => ResistBurnRate.RatedRandomBool();
        public bool RandomResistFrozing => ResistFrostRate.RatedRandomBool();

        public int ResistanceRate(DamageEffect effect) =>
            effect switch
            {
                DamageEffect.NA => 0,
                DamageEffect.Stunned => ResistStunRate,
                DamageEffect.Bleeding => ResistBleedRate,
                DamageEffect.Pierced => 0,
                DamageEffect.Burning => ResistBurnRate,
                DamageEffect.Frozing => ResistFrostRate,
                _ => 0,
            };

    }
}