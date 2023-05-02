using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleTryCastEffectsSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<DealEffectsTag> dealEffectsTagPool = default;

        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;

        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, DealEffectsTag>> filter = default;

        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;
        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                DealEffectsDamage(entity);
                dealEffectsTagPool.Value.Del(entity);
            }
        }

        private void DealEffectsDamage(int turnEntity)
        {

            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerEntity))
                throw new Exception("No attacker");

            if (!targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetEntity))
                throw new Exception("No target");

            ref var attackerConfig = ref battleService.Value.GetHeroConfig(attackerRef.HeroInstancePackedEntity);
            ref var targetConfig = ref battleService.Value.GetHeroConfig(targetRef.HeroInstancePackedEntity);

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            ref var currentRound = ref battleService.Value.CurrentRound;

            if (TryCast(
                attackerEntity, attackerConfig, 
                targetEntity, targetConfig, 
                currentRound.Round, 
                out var damageEffect))
            {
                if (damageEffect.Config.Effect == DamageEffect.Pierced)
                {
                    turnInfo.Pierced = true;
                }
                else
                {
                    turnInfo.TargetEffects = turnInfo.TargetEffects.Concat(
                        new DamageEffect[]{ damageEffect.Config.Effect }).ToArray();
                    turnInfo.ExtraDamage = damageEffect.Config.ExtraDamage;
                    turnInfo.Damage += turnInfo.ExtraDamage;

                    ref var effectsComp = ref effectsPool.Value.Get(targetEntity);
                    effectsComp.EnqueEffect(damageEffect);

                    ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
                    ref var healthComp = ref healthCompPool.Value.Get(targetEntity);

                    hpComp.Value = Mathf.Max(0, hpComp.Value - turnInfo.ExtraDamage);

                    ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(targetEntity);
                    barsAndEffectsComp.ActiveEffects = effectsComp.ActiveEffects;
                    barsAndEffectsComp.HealthCurrent = hpComp.Value;
                }
            }
        }

        private bool TryCast(
            int attackerEntity, Hero attackerConfig, 
            int targetEntity, Hero targetConfig, 
            int roundOn, 
            out DamageEffectInfo info)
        {
            DamageEffectConfig config = libraryService.Value.DamageTypesLibrary
                .ConfigForDamageType(attackerConfig.DamageType);

            info = default;

            if (config.Effect == DamageEffect.NA)
                return false;
            
            if (!prefs.Value.DisableRNGToggle)
            {
                var damageBlockedByRelations = ecsWorld.Value.GetDamageTypeBlock(targetEntity, attackerConfig.DamageType);
                if (damageBlockedByRelations)
                {
                    Debug.Log($"Damage Effect Blocked for {attackerConfig.DamageType} By Relations Effect!");
                    //TODO: communicate to UI/in-game logs
                    return false;
                }

                var adjustedChance = GetAdjustedChanceRate(attackerEntity, targetEntity, config.Effect, config.ChanceRate);
                if (!adjustedChance) return false;

                var adjustedResistance = GetAdjustedResistance(attackerEntity, targetEntity, config.Effect, targetConfig.ResistanceRate(config.Effect));                
                if (adjustedResistance) return false;                
            }

            DamageEffectInfo draft = DamageEffectInfo.Draft(config);

            draft = draft.SetDuration(roundOn, roundOn + config.TurnsActive);

            info = draft;

            return true;
        }

        private bool GetAdjustedResistance(int attackerEntity, int targetEntity, DamageEffect damageEffect, int rawResistRate)
        {
            //для атакуемого может быть изменена сопротивляемость через SpecKey + сопротивляемость
            var adjustment = ecsWorld.Value.GetRelationAdjustment(targetEntity, damageEffect.ResistanceSpec(), out var factor, out var value);
            var adjustedValue = adjustment switch
            {
                AdjustmentType.Factor => (int)(rawResistRate * factor),
                AdjustmentType.Value => value,
                _ => rawResistRate
            };

            return adjustedValue.RatedRandomBool();
        }

        private bool GetAdjustedChanceRate(int attackerEntity, int targetEntity, DamageEffect damageEffect, int rawChanceRate)
        {
            //(?) для атакуемого может быть увеличен шанс возникновения эффекта через DmgEffectBonusKey
            var key0 = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);
            var chanceRateAdj0 = ecsWorld.Value.GetRelationAdjustment(targetEntity, key0, out var factor0, out var value0);
            var adjustedValue0 = chanceRateAdj0 switch
            {
                AdjustmentType.Factor => (int)(rawChanceRate * factor0),
                AdjustmentType.Value => value0,
                _ => rawChanceRate
            };

            //для атакующего может быть увеличен шанс возникновения эффекта через DmgEffectBonusKey
            var key1 = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);
            var chanceRateAdj1 = ecsWorld.Value.GetRelationAdjustment(attackerEntity, key1, out var factor1, out var value1);
            var adjustedValue1 = chanceRateAdj1 switch
            {
                AdjustmentType.Factor => (int)(rawChanceRate * factor1),
                AdjustmentType.Value => value1,
                _ => rawChanceRate
            };

            //для атакующего может быть увеличен шанс возникновения конкретного эффекта через DmgEffectKey
            var key2 = new RelationEffectKey(SpecOption.NA, damageEffect, DamageType.NA, RelationsEffectType.DmgEffectKey);
            var chanceRateAdj2 = ecsWorld.Value.GetRelationAdjustment(attackerEntity, key2, out var factor2, out var value2);
            var adjustedValue2 = chanceRateAdj2 switch
            {
                AdjustmentType.Factor => (int)(rawChanceRate * factor2),
                AdjustmentType.Value => value2,
                _ => rawChanceRate
            };

            //TODO: выбрать что важнее из (adjustedValue 0-3) и применить
            var max = Mathf.Max(new int[] { rawChanceRate, adjustedValue0, adjustedValue1, adjustedValue2});
            return max.RatedRandomBool();
        }
    }
}
