using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDealAttackDamageSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;

        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;

        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool = default;


        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, DealDamageTag>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;
        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                DealDamage(entity);
                dealDamageTagPool.Value.Del(entity);
            }
        }

        private void DealDamage(int turnEntity)
        {

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);

            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerEntity))
                throw new Exception("No Attacker entity");

            if (!targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetEntity))
                throw new Exception("No Target entity");

            ref var attackerConfig = ref battleService.Value.GetHeroConfig(attackerRef.HeroInstancePackedEntity);

            int shield = GetAdjustedIntValue<DefenceRateTag>(targetEntity, SpecOption.DefenceRate);
            
            if (turnInfo.Pierced)
            {
                DamageEffectConfig config = libraryService.Value.DamageTypesLibrary
                    .ConfigForDamageType(attackerConfig.DamageType);
                shield = (int)(config.ShieldUseFactor / 100f * shield);
            }

            int rawDamage;
            if (prefs.Value.DisableRNGToggle)
            {
                var damageRange = ecsWorld.Value.ReadIntRangeValue<DamageRangeTag>(attackerEntity);
                rawDamage = damageRange.MaxRate;
            }
            else
            {
                rawDamage = GetAdjustedRangedValue<DamageRangeTag>(attackerEntity, SpecOption.DamageRange);
            }

            bool criticalDamage;
            if (prefs.Value.DisableRNGToggle)
            {
                criticalDamage = true;
            }
            else
            {
                criticalDamage = GetAdjustedBoolValue<CritRateTag>(attackerEntity, SpecOption.CritRate);
            }            

            int damage = rawDamage;
            damage *= criticalDamage ? 2 : 1;
            damage -= (int)(damage * shield / 100f);
            damage = Mathf.Max(1, damage); // no reason to deal 0 damage bc
                                           // this is only possible due to
                                           // roundings. changing 0 to 1

            turnInfo.Damage += damage;
            turnInfo.Critical = criticalDamage;

            ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
            ref var healthComp = ref healthCompPool.Value.Get(targetEntity);

            hpComp.Value = Mathf.Max(0, hpComp.Value - damage);

            ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(targetEntity);
            barsAndEffectsComp.HealthCurrent = hpComp.Value;
        }


        private bool GetAdjustedBoolValue<T>(int entity, SpecOption specOption)
            where T : struct
        {
            var adjValue = GetAdjustedIntValue<T>(entity, specOption);
            return adjValue.RatedRandomBool();
        }

        private int GetAdjustedIntValue<T>(int entity, SpecOption specOption)
            where T : struct
        {
            var rawValue = ecsWorld.Value.ReadIntValue<T>(entity);
            var adjType = GetAdjustmentValue(entity, specOption, out var factor, out var value);
            var adjValue = adjType switch
            {
                AdjustmentType.Factor => (int)(rawValue * factor),
                AdjustmentType.Value => value,
                _ => throw new NotImplementedException(),
            };
            return adjValue;
        }

        private int GetAdjustedRangedValue<T>(int entity, SpecOption specOption)
            where T : struct
        {
            var rawRange = ecsWorld.Value.ReadIntRangeValue<T>(entity);
            var rawValue = rawRange.RandomValue;
            var adjType = GetAdjustmentValue(entity, specOption, out var factor, out var value, rawRange);
            
            return adjType switch
            {
                AdjustmentType.Factor => (int)(rawValue * factor),
                AdjustmentType.Value => value,
                _ => throw new NotImplementedException(),
            };
        }

        private AdjustmentType GetAdjustmentValue(int entity, SpecOption specOption, out float factor, out int value, IntRange rangeValue = null)
        {
            var retval = AdjustmentType.Factor;

            value = 0;
            factor = 1f;
            
            ref var relationEffects = ref relEffectsPool.Value.Get(entity);

            foreach (var relEffect in relationEffects.CurrentEffects)
            {
                if (relEffect.Key.SpecOption != specOption)
                    continue;

                switch (relEffect.Value.Rule.EffectType)
                {
                    case RelationsEffectType.SpecMaxMin:
                        {
                            if (rangeValue == null)
                                throw new Exception("RelationsEffectType.SpecMaxMin requires ranged value");
                            
                            var rule = (EffectRuleSpecMaxMin)relEffect.Value.Rule;
                            retval = AdjustmentType.Value;
                            value = rule.MaxMin > 0 ? rangeValue.MaxRate : rangeValue.MinRate;
                        }
                        break;
                    case RelationsEffectType.SpecAbs:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Value.Rule;
                            retval = AdjustmentType.Factor;
                            factor += rule.Value / 100f;
                        }
                        break;
                    case RelationsEffectType.SpecPercent:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Value.Rule;
                            factor *= rule.Value / 100f;
                        }
                        break;
                    default:
                        break;
                }
            }

            return retval;
        }
    }
}
