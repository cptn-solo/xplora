﻿namespace Assets.Scripts.Data
{

    /// <summary>
    /// Refer to Hero fields (mostly matches field names)
    /// Only damage min/max should be converted to Range sctructure one day
    /// </summary>
    public enum SpecOption
    {
        NA = 0,
        DamageRange = 100, //Min-Max // DamageMin+DamageMax in Hero
        DefenceRate = 200,
        AccuracyRate = 300,
        DodgeRate = 400,
        CritRate = 450,
        Health = 500,
        Speed = 600,
        UnlimitedStaminaTag = 700, // not present in Hero fields (dynamic)
        HP = 800, // not present in Hero fields (dynamic)
        BleedingResistanceRate = 1100,
        PoisonResistanceRate = 1200,
        StunResistanceRate = 1300,
        BurningResistanceRate = 1400,
        FrozingResistanceRate = 1500,
        BlindnessResistanceRate = 1600,
    }
}