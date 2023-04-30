using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public struct DamageTypesLibrary
    {
        public Dictionary<DamageType, DamageEffectConfig> DamageTypes { get; private set; }
        public Dictionary<DamageEffect, DamageEffectConfig> DamageEffects { get; private set; }

        public static DamageTypesLibrary EmptyLibrary()
        {
            DamageTypesLibrary result = default;
            
            result.DamageTypes = new();
            result.DamageEffects = new();

            return result;
        }

        internal DamageEffectConfig ConfigForDamageType(DamageType damageType)
        {
            if (DamageTypes.TryGetValue(damageType, out var config))
                return config;

            return default;
        }
        internal DamageEffectConfig ConfigForDamageEffect(DamageEffect damageEffect)
        {
            if (DamageEffects.TryGetValue(damageEffect, out var config))
                return config;

            return default;
        }

        internal void UpdateConfig(
            DamageType damageType,
            DamageEffect damageEffect,
            int turnsCountInt,
            bool skipTurnsBool,
            int extraDamage, 
            int useShieldRate, 
            int chanceRate)
        {
            var config = DamageEffectConfig.Create(
                damageEffect, skipTurnsBool, turnsCountInt, extraDamage, useShieldRate, chanceRate);

            if (DamageEffects.TryGetValue(damageEffect, out _))
                DamageEffects[damageEffect] = config;
            else
                DamageEffects.Add(damageEffect, config);

            if (DamageTypes.TryGetValue(damageType, out _))
                DamageTypes[damageType] = config;
            else
                DamageTypes.Add(damageType, config);
        }
    }
}