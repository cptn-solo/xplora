using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRelEffectProbeSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<RelEffectProbeComp> pool = default;
        private readonly EcsPoolInject<EffectInstanceInfo> effectsPool = default;
        private readonly EcsPoolInject<DraftTag<EffectInstanceInfo>> draftTagPool = default;        
        
        private readonly EcsFilterInject<Inc<RelEffectProbeComp>> filter = default;
        
        private readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var comp = ref pool.Value.Get(entity);
                TryCastRelationEffect(
                    entity,
                    comp.SourceOrigPacked,
                    comp.SourceConfigRefPacked,
                    comp.TargetOrigPacked,
                    comp.TargetConfigRefPacked,
                    comp.SubjectState,
                    comp.P2PEntityPacked);
            }

        }

        /// <summary>
        /// Will add relation effect if rules applied will allow to
        /// </summary>
        /// <param name="effectProbeEntity">Entity of an effect probe between two heroes</param>
        /// <param name="effectSource">Packed other guy entity in the origin world</param>
        /// <param name="sourceConfigRefPacked">Hero Config Entity of the reletion who spawned the effect</param>
        /// <param name="effectTarget">Packed this guy entity in the origin world</param>
        /// <param name="targetConfigRefPacked">Hero Config Entity of the given hero, effect receiver</param>
        /// <param name="SubjectState">State of the effect target (attaking or being attacked</param>
        /// <param name="p2pEntityPacked">Packed score entity for the given hero and the other guy</param>
        /// <returns>true if new effect was casted</returns>
        private bool TryCastRelationEffect(
            int effectProbeEntity, 
            EcsPackedEntityWithWorld effectSource,
            EcsPackedEntityWithWorld sourceConfigRefPacked,
            EcsPackedEntityWithWorld effectTarget,
            EcsPackedEntityWithWorld targetConfigRefPacked,
            RelationSubjectState SubjectState,
            EcsPackedEntityWithWorld p2pEntityPacked)
        {
            if (!effectSource.Unpack(out var origWorld, out var effeectSourceEntity))
                throw new Exception("Stale Other Guy Entity (probably dead already)");

            if (!p2pEntityPacked.Unpack(out _, out var p2pEntity))
                throw new Exception("Stale Score Entity");

            var relationsConfig = heroLibraryService.Value.HeroRelationsConfigProcessor();
            var effectRules = heroLibraryService.Value.HeroRelationEffectsLibProcessor();

            var currentEffectsCount = origWorld.ReadIntValue<RelationEffectsCountTag>(p2pEntity);            
            // respect spawn rate from AdditionalEffectSpawnRate:
            if (!effectRules.TrySpawnAdditionalEffect(currentEffectsCount))
            {
                Debug.Log($"Spawn failed due to existing {currentEffectsCount} effects");
                return false;
            }

            var score = origWorld.ReadIntValue<RelationScoreTag>(p2pEntity);
            var relationsState = relationsConfig.GetRelationState(score);
            
            if (!targetConfigRefPacked.Unpack(out var libWorld, out var tgtHeroConfigEntity))
                throw new Exception("No Hero Config for current (recepient) guy");

            if (!sourceConfigRefPacked.Unpack(out _, out var srcHeroConfigEntity))
                throw new Exception("No Hero Config for other (spawner) guy");

            var libHeroPool = libWorld.GetPool<Hero>();
            ref var tgtHeroConfig = ref libHeroPool.Get(tgtHeroConfigEntity);
            ref var srcHeroConfig = ref libHeroPool.Get(srcHeroConfigEntity);

            var rulesCaseKey = new RelationEffectLibraryKey(
                srcHeroConfig.Id, SubjectState, relationsState);

            if (!effectRules.SubjectStateEffectsIndex.TryGetValue(rulesCaseKey, out var scope))
                return false; // no effect for relation state, it's ok

            var ruleType = scope.EffectRule.EffectType;

            if (ruleType.EffectClass() != RelationsEffectClass.Battle)
                return false; // this system process only battle effects

            var rule = (IBattleEffectRule)scope.EffectRule;
            ref var currentRound = ref battleService.Value.CurrentRound;

            Debug.Log($"Relations Effect of type {ruleType} was just spawned " +
                $"for {tgtHeroConfig.Name} in {scope.SelfState} due to {scope.RelationState} " +
                $"with {srcHeroConfig.Name}");

            // 1. add component to the entity of the current score data;
            // 2. keep all spawned effects (EffectRules) in that component;
            // 3. count of already spawned effects used as a wheight for spawn rate (key for 
            // AdditioinalEffectSpawnRate queries);

            draftTagPool.Value.Add(effectProbeEntity);
            
            ref var effect = ref effectsPool.Value.Add(effectProbeEntity);
            effect.StartRound = currentRound.Round;
            effect.EndRound = currentRound.Round + rule.TurnsCount;
            effect.UsageLeft = rule.TurnsCount;
            effect.Rule = rule;
            effect.EffectSource = effectSource;
            effect.EffectP2PEntity = p2pEntityPacked;

            Debug.Log($"Spawned with respect of exisiting {currentEffectsCount} effects");

            origWorld.IncrementIntValue<RelationEffectsCountTag>(1, p2pEntity);

            return true;
        }
    }
}
