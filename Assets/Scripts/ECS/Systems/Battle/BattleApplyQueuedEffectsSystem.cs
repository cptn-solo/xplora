using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{    
    /// <summary>
    /// Forwards effects attached to the subject (attacker/target) to the visualization pipeline 
    /// (via SubjectEffectsInfoComp) and decrements used effects
    /// </summary>
    /// <typeparam name="T">AttackerRef or TargetRef</typeparam>
    /// <typeparam name="E">AttackerEffectsTag or TargetEffectsTag</typeparam>
    public class BattleApplyQueuedEffectsSystem<T, E> : BaseEcsSystem
        where T : struct, IPackedWithWorldRef
        where E : struct
    {
        private readonly EcsPoolInject<T> subjectRefPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<EffectInstanceInfo> effectInstancePool = default;

        private readonly EcsPoolInject<E> subjectEffectsTagPool = default;
        private readonly EcsPoolInject<SubjectEffectsInfoComp> appliedEffectsCompPool = default;
        private readonly EcsPoolInject<ActiveEffectComp> activeEffectPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag, E>> filter = default;
        private readonly EcsFilterInject<
            Inc<ActiveEffectComp>, 
            Exc<SpecialDamageEffectTag>> effectsFilter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ApplyQueuedEffects(entity);
                subjectEffectsTagPool.Value.Del(entity);
            }
        }
        //private void ApplyQueuedEffects(BattleTurnInfo turnInfo, out Hero attacker, out BattleTurnInfo? effectsInfo)
        private void ApplyQueuedEffects(int turnEntity)
        {
            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            ref var subjectRef = ref subjectRefPool.Value.Get(turnEntity);

            if (!subjectRef.Packed.Unpack(out var world, out var subjectEntity))
                throw new Exception("No Subject");

            var buffer = ListPool<DamageEffect>.Get();
            var effectDamage = 0;
            foreach (var effEntity in effectsFilter.Value)
            {
                ref var effectComp = ref activeEffectPool.Value.Get(effEntity);
                if (!effectComp.Subject.EqualsTo(subjectRef.Packed))
                    continue;

                var resistanceFactor = GetResistanceFactor(subjectEntity, effectComp.Effect);
                effectDamage += (int)(resistanceFactor * libraryService.Value.DamageTypesLibrary
                    .ConfigForDamageEffect(effectComp.Effect).ExtraDamage);

                world.IncrementIntValue<DamageTag>(effectDamage, subjectEntity);
                buffer.Add(effectComp.Effect);

                // former "use effect":
                world.UseEffect(effEntity);
            }

            if (buffer.Count > 0)
            {
                if (!appliedEffectsCompPool.Value.Has(subjectEntity))
                    appliedEffectsCompPool.Value.Add(subjectEntity);
                
                ref var appliedEffects = ref appliedEffectsCompPool.Value.Get(subjectEntity);
                appliedEffects.SubjectEntity = subjectRef.Packed;
                appliedEffects.Effects = buffer.ToArray();
                appliedEffects.EffectsDamage = effectDamage;
            }

            ListPool<DamageEffect>.Add(buffer);
        }


        private float GetResistanceFactor(int entity, DamageEffect eff)
        {
            var retval = 1f; 
            ref var relationEffects = ref relEffectsPool.Value.Get(entity);
            foreach (var effectEntityPacked in relationEffects.CurrentEffects.Values)
            {
                if (!effectEntityPacked.Unpack(out var world, out var effectEntity))
                    throw new Exception("Stale rel effect instance");

                if (!effectInstancePool.Value.Has(effectEntity))
                    throw new Exception("No rel effect instance for entity");

                ref var relEffect = ref effectInstancePool.Value.Get(effectEntity);

                switch (relEffect.Rule.EffectType)
                {
                    case RelationsEffectType.SpecAbs:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Rule;
                            if (rule.SpecOption == eff.ResistanceSpec())
                                retval -= rule.Value / 100f;
                        }
                        break;
                    case RelationsEffectType.SpecPercent:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Rule;
                            if (rule.SpecOption == eff.ResistanceSpec())
                                retval *= rule.Value / 100f;
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
