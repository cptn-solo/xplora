using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignRelationEffectsSystem<T> : IEcsRunSystem
        where T : struct, IPackedWithWorldRef
    {
        protected virtual RelationSubjectState SubjectState => 
            RelationSubjectState.Attacking;

        protected readonly EcsWorldInject ecsWorld;
        
        protected readonly EcsPoolInject<T> pool = default;
        protected readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        protected readonly EcsPoolInject<HeroInstanceOriginRefComp> heroInstanceOriginRefPool = default;
        protected readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;
        protected readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        protected readonly EcsPoolInject<UpdateTag<RelationEffectInfo>> updatePool = default;
        protected readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        protected readonly EcsPoolInject<PrepareRevengeComp> revengePool = default;

        protected readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo, T>> filter = default;

        protected readonly EcsCustomInject<BattleManagementService> battleService = default;

        protected readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var heroInstanceRef = ref pool.Value.Get(entity);
                if (!heroInstanceRef.Packed.Unpack(out _, out var effectTargetEntity))
                    continue;

                if (!playerTeamTagPool.Value.Has(effectTargetEntity))
                    continue;

                ref var originRef = ref heroInstanceOriginRefPool.Value.Get(effectTargetEntity);
                if (!originRef.Packed.Unpack(out var origWorld, out var effectTargetOrig))
                    continue;

                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(effectTargetEntity);

                var matrixFilter = origWorld.Filter<RelationsMatrixComp>().End();
                var matrixPool = origWorld.GetPool<RelationsMatrixComp>();

                foreach (var matrixEntity in matrixFilter)
                {
                    ref var matrixComp = ref matrixPool.Get(matrixEntity);
                    foreach (var item in matrixComp.Matrix)
                    {
                        // matching only one side of the key to avoid duplicates
                        if (!item.Key.Item1.EqualsTo(originRef.Packed))
                            continue;

                        if (TryCastRelationEffect(heroConfigRef.Packed, item.Key.Item2, originRef.Packed, item.Value, 
                        out var effect))
                        {
                            // registering effect for the hero affected (in the battle world, to make it handy when needed) 
                            RelationEffectKey ruleKey = effect.Rule.Key;
                            item.Key.Item2.Unpack(out _, out var effectSourceEntity);

                            var affectedParty = effectTargetEntity;

                            if (ruleKey.RelationsEffectType switch { 
                                RelationsEffectType.AlgoRevenge => true,
                                RelationsEffectType.AlgoTarget => true,
                                _ => false
                                })
                            {
                                affectedParty = effectSourceEntity;

                                ref var attackerRef = ref attackerRefPool.Value.Get(entity);
                                effect.EffectFocus = attackerRef.Packed;

                                var revengeEntity = ecsWorld.Value.NewEntity();                            
                                ref var revengeComp = ref revengePool.Value.Add(revengeEntity);
                                revengeComp.RevengeBy = ecsWorld.Value.PackEntityWithWorld(effectSourceEntity);
                                revengeComp.RevengeFor = ecsWorld.Value.PackEntityWithWorld(effectTargetEntity);
                            }

                            ref var relEffects = ref relEffectsPool.Value.Get(affectedParty);
                            relEffects.SetEffect(ruleKey, effect);
                        
                            if (!updatePool.Value.Has(affectedParty))
                                updatePool.Value.Add(affectedParty);
                        }                        

                    }
                }

            }
        }

        /// <summary>
        /// Will add relation effect if rules applied will allow to
        /// </summary>
        /// <param name="targetEntity">Entity of a hero to wich the effect will be casted (if any)</param>
        /// <param name="origWorld">EcsWorld to take relations from</param>
        /// <param name="heroConfigPackedEntity">Hero Config Entity of the given hero</param>
        /// <param name="effectSource">Packed other guy entity in the origin world</param>
        /// <param name="effectTarget">Packed this guy entity in the origin world</param>
        /// <param name="scoreEntityPacked">Packed score entity for the given hero and the other guy</param>
        /// <returns>true if new effect was casted</returns>
        protected bool TryCastRelationEffect(
            EcsPackedEntityWithWorld heroConfigPackedEntity,
            EcsPackedEntityWithWorld effectSource,
            EcsPackedEntityWithWorld effectTarget,
            EcsPackedEntityWithWorld scoreEntityPacked,
            out EffectInstanceInfo effect)
        {
            effect = default;
            if (!effectSource.Unpack(out var origWorld, out var otherGuyEntity))
                throw new Exception("Stale Other Guy Entity (probably dead already)");

            if (!scoreEntityPacked.Unpack(out _, out var scoreEntity))
                throw new Exception("Stale Score Entity");

            var relationsConfig = heroLibraryService.Value.HeroRelationsConfigProcessor();

            var score = origWorld.ReadIntValue<RelationScoreTag>(scoreEntity);
            var relationsState = relationsConfig.GetRelationState(score);

            
            if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero Config for current guy");

            var libHeroPool = libWorld.GetPool<Hero>();
            ref var heroConfig = ref libHeroPool.Get(heroConfigEntity);

            var rulesCaseKey = new RelationEffectLibraryKey(
                heroConfig.Id, SubjectState, relationsState);

            var effectRules = heroLibraryService.Value.HeroRelationEffectsLibProcessor();

            if (!effectRules.SubjectStateEffectsIndex.TryGetValue(rulesCaseKey, out var scope))
                return false; // no effect for relation state, it's ok

            var origWorldConfigRefPool = origWorld.GetPool<HeroConfigRefComp>();
            ref var otherGuyConfigRef = ref origWorldConfigRefPool.Get(otherGuyEntity);
            if (!otherGuyConfigRef.Packed.Unpack(out _, out var otherGuyHeroConfigEntity))
                throw new Exception("No Hero Config for the other guy");

            ref var otherGuyHeroConfig = ref libHeroPool.Get(otherGuyHeroConfigEntity);

            var ruleType = scope.EffectRule.EffectType;

            if (ruleType.EffectClass() != RelationsEffectClass.Battle)
                return false; // this system process only battle effects

            var rule = (IBattleEffectRule)scope.EffectRule;
            ref var currentRound = ref battleService.Value.CurrentRound;

            Debug.Log($"Relations Effect of type {ruleType} was just spawned " +
                $"for {heroConfig.Name} in {scope.SelfState} due to {scope.RelationState} " +
                $"with {otherGuyHeroConfig.Name}");

            // 1. add component to the entity of the current score data;
            // 2. keep all spawned effects (EffectRules) in that component;
            // 3. count of already spawned effects used as a wheight for spawn rate (key for 
            // AdditioinalEffectSpawnRate queries);

            var currentEffectsPool = origWorld.GetPool<RelationEffectsComp>();            
            // respect spawn rate from AdditionalEffectSpawnRate:
            ref var currentEffects = ref currentEffectsPool.Get(scoreEntity);
            if (!effectRules.TrySpawnAdditionalEffect(currentEffects.CurrentEffects.Count))
                return false;

            Debug.Log($"Spawned with respect of exisiting {currentEffects.CurrentEffects.Count} effects");
                        
            effect = new EffectInstanceInfo()
            {
                StartRound = currentRound.Round,
                EndRound = currentRound.Round + rule.TurnsCount,
                UsageLeft = rule.TurnsCount,
                Rule = rule,
                EffectSource = effectSource,
            };

            currentEffects.SetEffect(rule.Key, effect); // effect focus (if any) is omitted here, but added for battle context.

            return true;
        }
    }
}
