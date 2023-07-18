using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleSceneVisualsQueuedEffectsSystem : IEcsRunSystem
    {

        #region pools

        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<AttackerEffectsInfoComp> appliedEffectsCompPool = default;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<DeadTag> deadTagPool = default;

        #endregion

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag, AttackerRef, AttackerEffectsInfoComp>,
            Exc<AwaitingVisualsTag>> filter = default;

        // attacker effects were scheduled:
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var turnEntity in filter.Value)
            {
                ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

                if (!attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerEntity))
                    throw new Exception("No Attacker entity");

                // if effects were queued for the attacker:
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
                    ref var hpComp = ref hpCompPool.Value.Get(effectsSubject);
                    ref var healthComp = ref healthCompPool.Value.Get(effectsSubject);

                    // can be deprecated actually if separate comps used for bars and effects
                    ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(effectsSubject);
                    barsAndEffectsComp.Health = healthComp.Value;
                    barsAndEffectsComp.HealthCurrent = hpComp.Value;
                    barsAndEffectsComp.SetInstantEffects(appliedEffects.Effects);

                    ref var effectsBarEffects = ref world.ScheduleSceneVisuals<EffectsBarVisualsInfo>(turnEntity);
                    effectsBarEffects.SubjectEntity = effects.SubjectEntity;
                    effectsBarEffects.ActiveEffects = barsAndEffectsComp.ActiveEffects;

                    ref var hpBarEffects = ref world.ScheduleSceneVisuals<HealthBarVisualsInfo>(turnEntity);
                    hpBarEffects.SubjectEntity = effects.SubjectEntity;
                    hpBarEffects.Health = healthComp.Value;
                    hpBarEffects.HealthCurrent = hpComp.Value;
                    hpBarEffects.Damage = effects.EffectsDamage;
                    hpBarEffects.BarsInfoBattle = barsAndEffectsComp.BarsInfoBattle;
                }

                // if attacker hasn't survived effects
                if (deadTagPool.Value.Has(attackerEntity))
                {
                    ref var deathEffects = ref world.ScheduleSceneVisuals<DeathVisualsInfo>(turnEntity);
                    deathEffects.SubjectEntity = attackerRef.Packed;
                }
                else
                {
                    {
                        // current effects if exist (or cleanup if not):
                        ref var effectsComp = ref effectsPool.Value.Get(attackerEntity);
                        ref var effectsBarEffects = ref world.ScheduleSceneVisuals<EffectsBarVisualsInfo>(turnEntity);
                        effectsBarEffects.SubjectEntity = attackerRef.Packed;
                        effectsBarEffects.ActiveEffects = effectsComp.ActiveEffects;
                    }
                }
            }
        }

    }
}
