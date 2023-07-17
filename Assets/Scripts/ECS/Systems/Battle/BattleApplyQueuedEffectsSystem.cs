using System;
using System.Linq;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleApplyQueuedEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;

        private readonly EcsPoolInject<AttackerEffectsTag> attackerEffectsTagPool = default;
        private readonly EcsPoolInject<AttackerEffectsInfoComp> appliedEffectsCompPool = default;
        private readonly EcsPoolInject<AttackTag> attackTagPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag, AttackerEffectsTag>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ApplyQueuedEffects(entity);
                attackerEffectsTagPool.Value.Del(entity);
            }
        }
        //private void ApplyQueuedEffects(BattleTurnInfo turnInfo, out Hero attacker, out BattleTurnInfo? effectsInfo)
        private void ApplyQueuedEffects(int turnEntity)
        {
            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out var world, out var attackerEntity))
                throw new Exception("No Attacker");

            ref var effectsComp = ref effectsPool.Value.Get(attackerEntity);
            var effs = effectsComp.ActiveEffects.Keys.ToArray(); // will be used to flash used effects ===>

            var effectDamage = 0;
            foreach (var eff in effs)
            {
                var resistanceFactor = GetResistanceFactor(attackerEntity, eff);
                effectDamage += (int)(resistanceFactor * libraryService.Value.DamageTypesLibrary
                    .ConfigForDamageEffect(eff).ExtraDamage);
                effectsComp.UseEffect(eff, out var used);
            }

            ref var hpComp = ref hpCompPool.Value.Get(attackerEntity);
            ref var healthComp = ref healthCompPool.Value.Get(attackerEntity);

            hpComp.Value = Mathf.Max(0, hpComp.Value - effectDamage);

            // intermediate turn info, no round turn override to preserve pre-calculated target:
            var lethal = hpComp.Value <= 0;

            ref var appliedEffects = ref appliedEffectsCompPool.Value.Add(turnEntity);
            appliedEffects.SubjectEntity = turnInfo.Attacker.Value;
            appliedEffects.Effects = effs;
            appliedEffects.EffectsDamage = effectDamage;
            appliedEffects.Lethal = lethal;

            if (lethal)
                attackTagPool.Value.Del(turnEntity);
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
