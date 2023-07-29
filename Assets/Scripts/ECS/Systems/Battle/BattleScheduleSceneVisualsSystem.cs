using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleSceneVisualsSystem : BaseEcsSystem
    {
        #region pools
        
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<SubjectEffectsInfoComp> appliedEffectsCompPool = default;
        private readonly EcsPoolInject<DeadTag> deadTagPool = default;
        private readonly EcsPoolInject<RangedTag> rangedTagPool = default;
        private readonly EcsPoolInject<TransformRef<VisualsTransformTag>> transformRefPool = default;

        #endregion

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag, AttackerRef, TargetRef>,
            Exc<AwaitingVisualsTag>> filter = default;
        

        // not skipped (both attacker and target present):
        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var turnEntity in filter.Value)
            {
                ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
                ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

                if (!attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerEntity))
                    throw new Exception("No Attacker entity");

                if (!targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetEntity))
                    throw new Exception("No Target entity");

                // attack:
                var ranged = rangedTagPool.Value.Has(attackerEntity);
                ref var targetTransformRef = ref transformRefPool.Value.Get(targetEntity);
                var targetTransform = targetTransformRef.Transform;

                // if attacker not ranged:
                if (!ranged)
                {
                    ref var moveVisualsInfo = ref world.ScheduleSceneVisuals<AttackMoveVisualsInfo>(turnEntity);
                    moveVisualsInfo.SubjectEntity = attackerRef.Packed;
                    moveVisualsInfo.TargetEntity = targetRef.Packed;

                    moveVisualsInfo.TargetTransform = targetTransform;
                }

                // attack:
                ref var attackVisualsInfo = ref world.ScheduleSceneVisuals<AttackerAttackVisualsInfo>(turnEntity);
                attackVisualsInfo.SubjectEntity = attackerRef.Packed;
                attackVisualsInfo.TargetEntity = targetRef.Packed;
                attackVisualsInfo.Ranged = ranged;
                attackVisualsInfo.TargetTransform = targetTransform;


                // if dodged:
                if (world.CheckForActiveEffect<DodgedTag>(targetEntity))
                {
                    ref var dodgeVisualsInfo = ref world.ScheduleSceneVisuals<AttackDodgeVisualsInfo>(turnEntity);
                    dodgeVisualsInfo.SubjectEntity = targetRef.Packed;
                }
                // if not dodged:
                else
                {
                    ref var hitVisualsInfo = ref world.ScheduleSceneVisuals<HitVisualsInfo>(turnEntity);
                    hitVisualsInfo.SubjectEntity = targetRef.Packed;

                    // if target effects (esp. armor pierced)
                    if (world.CheckForActiveEffect<PiercedTag>(targetEntity))
                    {
                        ref var piercedEffectVisualsInfo = ref world.ScheduleSceneVisuals<ArmorPiercedVisualsInfo>(turnEntity);
                        piercedEffectVisualsInfo.SubjectEntity = targetRef.Packed;
                        piercedEffectVisualsInfo.Damage = world.ReadIntValue<DamageTag>(targetEntity);
                    }
                    else if (appliedEffectsCompPool.Value.Has(targetEntity))
                    {

                        ref var currentEffectsComp = ref appliedEffectsCompPool.Value.Get(targetEntity);
                        ref var appliedEffects = ref world.ScheduleSceneVisuals<DamageEffectVisualsInfo>(turnEntity);
                        appliedEffects.SubjectEntity = targetRef.Packed;
                        appliedEffects.Effects = currentEffectsComp.Effects;
                        // probably should ignore damage while visualizing effects as we do have a dedicated stage for damage (bars) later
                        appliedEffects.EffectsDamage = currentEffectsComp.EffectsDamage;

                        ref var efffectsBarEffects = ref world.ScheduleSceneVisuals<EffectsBarVisualsInfo>(turnEntity);
                        efffectsBarEffects.SubjectEntity = targetRef.Packed;
                        efffectsBarEffects.InstantEffects = currentEffectsComp.Effects;
                    }

                    // if damage dealt
                    var currentDamage = world.ReadIntValue<DamageTag>(targetEntity);
                    if (currentDamage > 0)
                    {
                        ref var damageVisualsInfo = ref world.ScheduleSceneVisuals<TakingDamageVisualsInfo>(turnEntity);
                        damageVisualsInfo.SubjectEntity = targetRef.Packed;
                        damageVisualsInfo.Damage = currentDamage;
                        damageVisualsInfo.Critical = world.CheckForActiveEffect<CriticalTag>(targetEntity);
                        damageVisualsInfo.Lethal = world.CheckForActiveEffect<LethalTag>(targetEntity);

                        ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
                        ref var healthComp = ref healthCompPool.Value.Get(targetEntity);

                        // can be deprecated actually if separate comps used for bars and effects
                        ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(targetEntity);
                        barsAndEffectsComp.Health = healthComp.Value;
                        barsAndEffectsComp.HealthCurrent = hpComp.Value;

                        ref var hpBarEffects = ref world.ScheduleSceneVisuals<HealthBarVisualsInfo>(turnEntity);
                        hpBarEffects.SubjectEntity = targetRef.Packed;
                        hpBarEffects.Health = healthComp.Value;
                        hpBarEffects.HealthCurrent = hpComp.Value;
                        hpBarEffects.Damage = currentDamage;
                        hpBarEffects.BarsInfoBattle = barsAndEffectsComp.BarsInfoBattle;
                    }
                }

                if (deadTagPool.Value.Has(targetEntity))
                {
                    ref var deathEffects = ref world.ScheduleSceneVisuals<DeathVisualsInfo>(turnEntity);
                    deathEffects.SubjectEntity = targetRef.Packed;
                }
                else
                {
                    var activeEffects = world.GetActiveEffects(targetRef.Packed);
                    // current effects if exist (or cleanup if not):
                    ref var effectsBarEffects = ref world.ScheduleSceneVisuals<EffectsBarVisualsInfo>(turnEntity);
                    effectsBarEffects.SubjectEntity = targetRef.Packed;
                    effectsBarEffects.ActiveEffects = activeEffects;
                }

                if (!ranged)
                {
                    ref var moveBackEffects = ref world.ScheduleSceneVisuals<AttackerMoveBackVisualsInfo>(turnEntity);
                    moveBackEffects.SubjectEntity = attackerRef.Packed;
                }
            }
        }              
    }
}
