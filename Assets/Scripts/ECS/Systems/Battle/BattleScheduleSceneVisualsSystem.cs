using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Battle;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEditor.Build;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleSceneVisualsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<DeadTag> deadTagPool = default;
        private readonly EcsPoolInject<RangedTag> rangedTagPool = default;
        private readonly EcsPoolInject<TransformRef<VisualsTransformTag>> transformRefPool = default;

        private readonly EcsPoolInject<ScheduleVisualsTag> schedulePool = default;
        private readonly EcsPoolInject<AwaitingVisualsTag> waitPool = default;
        
        private readonly EcsPoolInject<AttackerEffectsInfoComp> appliedEffectsCompPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag>,
            Exc<AwaitingVisualsTag>> filter = default;

        public void Run(IEcsSystems systems)
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

                // if effects were queued for the attacker:
                if (appliedEffectsCompPool.Value.Has(turnEntity))
                {
                    ref var effects = ref appliedEffectsCompPool.Value.Get(turnEntity);

                    ref var hitVisualsInfo = ref world.ScheduleSceneVisuals<HitVisualsInfo>(turnEntity);
                    hitVisualsInfo.SubjectEntity = effects.SubjectEntity;

                    ref var appliedEffects = ref world.ScheduleSceneVisuals<DamageEffectVisualsInfo>(turnEntity);
                    appliedEffects.SubjectEntity = effects.SubjectEntity;
                    appliedEffects.Effects = effects.Effects;
                    appliedEffects.EffectsDamage = effects.EffectsDamage;
                    appliedEffects.Lethal = effects.Lethal;

                    if (effects.SubjectEntity.Unpack(out _, out var effectsSubject))
                    {
                        ref var effectsBarEffects = ref world.ScheduleSceneVisuals<EffectsBarVisualsInfo>(turnEntity);
                        ref var effectsComp = ref effectsPool.Value.Get(effectsSubject);
                        effectsBarEffects.SubjectEntity = effects.SubjectEntity;
                        effectsBarEffects.ActiveEffects = effectsComp.ActiveEffects;

                        ref var hpComp = ref hpCompPool.Value.Get(effectsSubject);
                        ref var hpBarEffects = ref world.ScheduleSceneVisuals<HealthBarVisualsInfo>(turnEntity);
                        hpBarEffects.SubjectEntity = effects.SubjectEntity;
                        hpBarEffects.HealthCurrent = hpComp.Value;
                        hpBarEffects.Damage = effects.EffectsDamage;
                    }
                }

                // if attacker hasn't survived effects
                if (deadTagPool.Value.Has(attackerEntity))
                {
                    ref var deathEffects = ref world.ScheduleSceneVisuals<DeathVisualsInfo>(turnEntity);
                    deathEffects.SubjectEntity = attackerRef.Packed;
                }
                else
                {
                    var ranged = rangedTagPool.Value.Has(attackerEntity);
                    ref var targetTransformRef = ref transformRefPool.Value.Get(targetEntity);
                    var targetTransform = targetTransformRef.Transform;
                    
                    // if attacker not ranged:
                    if (!ranged) {
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
                    if (turnInfo.Dodged)
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
                        if (turnInfo.Pierced)
                        {
                            ref var piercedEffectVisualsInfo = ref world.ScheduleSceneVisuals<ArmorPiercedVisualsInfo>(turnEntity);
                            piercedEffectVisualsInfo.SubjectEntity = targetRef.Packed;
                            piercedEffectVisualsInfo.Damage = turnInfo.Damage;
                        }
                        else if (turnInfo.TargetEffects.Length > 0)
                        {
                            ref var appliedEffects = ref world.ScheduleSceneVisuals<DamageEffectVisualsInfo>(turnEntity);
                            appliedEffects.SubjectEntity = targetRef.Packed;
                            appliedEffects.Effects = turnInfo.TargetEffects;
                            // probably should ignore damage while visualizing effects as we do have a dedicated stage for damage (bars) later
                            appliedEffects.EffectsDamage = turnInfo.ExtraDamage;
                            appliedEffects.Lethal = turnInfo.Lethal;

                            ref var efffectsBarEffects = ref world.ScheduleSceneVisuals<EffectsBarVisualsInfo>(turnEntity);
                            ref var effectsComp = ref effectsPool.Value.Get(targetEntity);
                            efffectsBarEffects.SubjectEntity = targetRef.Packed;
                            efffectsBarEffects.InstantEffects = turnInfo.TargetEffects;
                        }

                        // if damage dealt
                        if (turnInfo.Damage > 0)
                        {
                            ref var damageVisualsInfo = ref world.ScheduleSceneVisuals<TakingDamageVisualsInfo>(turnEntity);
                            damageVisualsInfo.SubjectEntity = targetRef.Packed;
                            damageVisualsInfo.Damage = turnInfo.Damage;
                            damageVisualsInfo.Critical = turnInfo.Critical;
                            damageVisualsInfo.Lethal = turnInfo.Lethal;

                            ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
                            ref var hpBarEffects = ref world.ScheduleSceneVisuals<HealthBarVisualsInfo>(turnEntity);
                            hpBarEffects.SubjectEntity = targetRef.Packed;
                            hpBarEffects.HealthCurrent = hpComp.Value;
                            hpBarEffects.Damage = turnInfo.Damage;
                        }
                    }

                    if (deadTagPool.Value.Has(targetEntity))
                    {
                        ref var deathEffects = ref world.ScheduleSceneVisuals<DeathVisualsInfo>(turnEntity);
                        deathEffects.SubjectEntity = targetRef.Packed;
                    }
                }

                // prevent reuse:
                schedulePool.Value.Del(turnEntity);
                waitPool.Value.Add(turnEntity);
            }

        }
    }
}
