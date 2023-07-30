﻿namespace Assets.Scripts.Data
{
    public enum DamageEffect
    {
        NA = 0,
        Stunned = 100,
        Bleeding = 200,
        Pierced = 300,
        Burning = 400,
        Frozing = 500,
        
        Critical = 1000, //special case to unify processing
        Lethal = 1010, //special case to unify processing
        Raw = 1020, //special case to unify processing
    }
}