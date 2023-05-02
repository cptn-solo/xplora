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
                if (key.SpecOption != SpecOption.NA && relEffect.Key.SpecOption == key.SpecOption)
                {
                    switch (relEffect.Value.Rule.EffectType)
                    {
                        case RelationsEffectType.SpecMaxMin:
                            {
                                if (rangeValue == null)
                                    throw new Exception("RelationsEffectType.SpecMaxMin requires ranged value");

                                var rule = (EffectRuleSpecMaxMin)relEffect.Value.Rule;
                                value = rule.MaxMin > 0 ? rangeValue.MaxRate : rangeValue.MinRate;
                                return AdjustmentType.Value;
                            }
                        case RelationsEffectType.SpecAbs:
                            {
                                var rule = (EffectRuleSpecAbs)relEffect.Value.Rule;
                                factor += rule.Value / 100f;
                                return AdjustmentType.Factor;
                            }
                        case RelationsEffectType.SpecPercent:
                            {
                                var rule = (EffectRuleSpecAbs)relEffect.Value.Rule;
                                factor *= rule.Value / 100f;
                                return AdjustmentType.Factor;
                            }
                        default:
                            break;
                    }
                }
                else if (false)
                {            
                    //var key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, damageType, RelationsEffectType.AlgoDamageTypeBlock);

                    //TODO: (?) для атакуемого может быть увеличен шанс возникновения эффекта через DmgEffectBonusKey
                    //var key0 = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);

                    //TODO: для атакующего может быть увеличен шанс возникновения эффекта через DmgEffectBonusKey
                    //var key1 = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);

                    //TODO: для атакующего может быть увеличен шанс возникновения конкретного эффекта через DmgEffectKey
                    //var key2 = new RelationEffectKey(SpecOption.NA, damageEffect, DamageType.NA, RelationsEffectType.DmgEffectKey);

                }
                else
                {
                    continue;
                }
            }

            return AdjustmentType.NA;
        }

    }
}
