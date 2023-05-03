using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS
{
    public static class EcsRelationEffectsExtensions
    {
        public static bool GetAdjustedBoolValue<T>(this EcsWorld ecsWorld, int entity, SpecOption specOption)
            where T : struct
        {
            var adjValue = ecsWorld.GetAdjustedIntValue<T>(entity, specOption);
            return adjValue.RatedRandomBool();
        }

        public static int GetAdjustedIntValue<T>(this EcsWorld ecsWorld, int entity, SpecOption specOption)
            where T : struct
        {
            var rawValue = ecsWorld.ReadIntValue<T>(entity);
            var adjType = ecsWorld.GetRelationAdjustment(entity, specOption, out var factor, out var value);
            var adjValue = adjType switch
            {
                AdjustmentType.Factor => (int)(rawValue * factor),
                AdjustmentType.Value => value,
                _ => rawValue,
            };
            return adjValue;
        }

        public static int GetAdjustedRangedValue<T>(this EcsWorld ecsWorld, int entity, SpecOption specOption)
            where T : struct
        {
            var rawRange = ecsWorld.ReadIntRangeValue<T>(entity);
            var rawValue = rawRange.RandomValue;
            var adjType = ecsWorld.GetRelationAdjustment(entity, specOption, out var factor, out var value, rawRange);

            return adjType switch
            {
                AdjustmentType.Factor => (int)(rawValue * factor),
                AdjustmentType.Value => value,
                _ => rawValue,
            };
        }
        
        public static bool GetDamageTypeBlock(this EcsWorld ecsWorld, int entity, DamageType damageType)
        {
            var key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, damageType, RelationsEffectType.AlgoDamageTypeBlock);
            var adjType = ecsWorld.GetRelationAdjustment(entity, key, out _, out var value);
            var adjValue = adjType switch
            {
                AdjustmentType.Value => value == 1,
                _ => false,
            };
            return adjValue;
        }

        public static AdjustmentType GetDamageEffect(this EcsWorld ecsWorld, int entity,
            DamageEffect damageEffect, out float factor, out int value, IntRange rangeValue = null)
        {
            var key = new RelationEffectKey(SpecOption.NA, damageEffect, DamageType.NA, RelationsEffectType.DmgEffectKey);
            return ecsWorld.GetRelationAdjustment(entity, key, out factor, out value, rangeValue);
        }

        public static AdjustmentType GetDamageEffectBonus(this EcsWorld ecsWorld, int entity,
            out float factor, out int value, IntRange rangeValue = null)
        {
            var key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);
            return ecsWorld.GetRelationAdjustment(entity, key, out factor, out value, rangeValue);
        }

        public static AdjustmentType GetRelationAdjustment(this EcsWorld ecsWorld, int entity,
            SpecOption specOption, out float factor, out int value, IntRange rangeValue = null)
        {
            var key = new RelationEffectKey(specOption, DamageEffect.NA, DamageType.NA, RelationsEffectType.SpecKey);
            return ecsWorld.GetRelationAdjustment(entity, key, out factor, out value, rangeValue);
        }

        public static AdjustmentType GetRelationAdjustment(this EcsWorld ecsWorld, int entity,
            RelationEffectKey key, out float factor, out int value, IntRange rangeValue = null)
        {
            value = 0;
            factor = 1f;

            ref var relationEffects = ref ecsWorld.GetPool<RelationEffectsComp>().Get(entity);

            foreach (var relEffect in relationEffects.CurrentEffects)
            {
                switch (key.RelationsEffectType)
                {
                    case RelationsEffectType.SpecKey:
                        if (key.SpecOption != SpecOption.NA && relEffect.Key.SpecOption == key.SpecOption)
                            return GetSpecKeyAdjustment(relEffect.Value, out factor, out value, rangeValue);
                        break;
                    case RelationsEffectType.DmgEffectKey:
                        if (key.DamageEffect != DamageEffect.NA && relEffect.Key.DamageEffect == key.DamageEffect)
                            return GetDmgEffectKeyAdjustment(relEffect.Value, out factor, out value);
                        break;
                    case RelationsEffectType.DmgEffectBonusKey:
                        if (relEffect.Key.RelationsEffectType switch
                        {
                            RelationsEffectType.DmgEffectBonusAbs => true,
                            RelationsEffectType.DmgEffectBonusPercent => true,
                            _ => false
                        }) return GetDmgEffectBonusKeyAdjustment(relEffect.Value, out factor, out value);
                        break;
                    case RelationsEffectType.AlgoDamageTypeBlock:
                        if (key.DamageType != DamageType.NA && relEffect.Key.DamageType == key.DamageType)
                            return GetAlgoDamageTypeBlockAdjustment(relEffect.Value, out value);
                        break;
                    case RelationsEffectType.AlgoRevenge:
                        //TODO:
                        break;
                    case RelationsEffectType.AlgoTarget:
                        //TODO:
                        break;
                    default:
                        //actually invalid entry as we are always know the key type exactly
                        break;
                }
            }

            return AdjustmentType.NA;
        }

        private static AdjustmentType GetAlgoDamageTypeBlockAdjustment(EffectInstanceInfo relEffect, out int value)
        {
            value = 0;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.AlgoDamageTypeBlock:
                    {
                        var rule = (EffectRuleAlgoDamageTypeBlock)relEffect.Rule;
                        value = rule.Flag;
                        return AdjustmentType.Value;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }

        private static AdjustmentType GetDmgEffectBonusKeyAdjustment(EffectInstanceInfo relEffect, out float factor, out int value)
        {
            value = 0;
            factor = 1f;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.DmgEffectBonusAbs:
                    {
                        var rule = (EffectRuleDmgEffectBonusAbs)relEffect.Rule;
                        factor += rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                case RelationsEffectType.DmgEffectBonusPercent:
                    {
                        var rule = (EffectRuleDmgEffectBonusPercent)relEffect.Rule;
                        factor *= rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }

        private static AdjustmentType GetDmgEffectKeyAdjustment(EffectInstanceInfo relEffect, out float factor, out int value)
        {
            value = 0;
            factor = 1f;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.DmgEffectAbs:
                    {
                        var rule = (EffectRuleDmgEffectAbs)relEffect.Rule;
                        factor += rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                case RelationsEffectType.DmgEffectPercent:
                    {
                        var rule = (EffectRuleDmgEffectPercent)relEffect.Rule;
                        factor *= rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }

        private static AdjustmentType GetSpecKeyAdjustment(EffectInstanceInfo relEffect, out float factor, out int value, IntRange rangeValue = null)
        {
            value = 0;
            factor = 1f;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.SpecMaxMin:
                    {
                        if (rangeValue == null)
                            throw new Exception("RelationsEffectType.SpecMaxMin requires ranged value");

                        var rule = (EffectRuleSpecMaxMin)relEffect.Rule;
                        value = rule.MaxMin > 0 ? rangeValue.MaxRate : rangeValue.MinRate;
                        return AdjustmentType.Value;
                    }
                case RelationsEffectType.SpecAbs:
                    {
                        var rule = (EffectRuleSpecAbs)relEffect.Rule;
                        factor += rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                case RelationsEffectType.SpecPercent:
                    {
                        var rule = (EffectRuleSpecAbs)relEffect.Rule;
                        factor *= rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }
    }
}
