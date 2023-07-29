using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// 
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

        private readonly EcsPoolInject<E> subjectEffectsTagPool = default;
        private readonly EcsPoolInject<SubjectEffectsInfoComp> appliedEffectsCompPool = default;
        private readonly EcsPoolInject<ActiveEffectComp> activeEffectPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag, E>> filter = default;
        private readonly EcsFilterInject<Inc<ActiveEffectComp>> effectsFilter = default;

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
                activeEffectPool.Value.Del(effEntity); // TODO: cleanup hero effect after turn ends
            }

            if (buffer.Count > 0)
            {
                ref var appliedEffects = ref appliedEffectsCompPool.Value.Add(subjectEntity);
                appliedEffects.SubjectEntity = turnInfo.Attacker.Value;
                appliedEffects.Effects = buffer.ToArray();
                appliedEffects.EffectsDamage = effectDamage;
            }

            ListPool<DamageEffect>.Add(buffer);
        }


        private float GetResistanceFactor(int entity, DamageEffect eff)
        {
            var retval = 1f; 
            ref var relationEffects = ref relEffectsPool.Value.Get(entity);
            foreach (var relEffect in relationEffects.CurrentEffects)
            {
                switch (relEffect.Value.Rule.EffectType)
                {
                    case RelationsEffectType.SpecAbs:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Value.Rule;
                            if (rule.SpecOption == eff.ResistanceSpec())
                                retval -= rule.Value / 100f;
                        }
                        break;
                    case RelationsEffectType.SpecPercent:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Value.Rule;
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
