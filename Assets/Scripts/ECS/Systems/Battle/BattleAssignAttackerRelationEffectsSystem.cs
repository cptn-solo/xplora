using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignAttackerRelationEffectsSystem : IEcsRunSystem
    {        
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> heroInstanceOriginRefPool = default;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo, AttackerRef>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(entity);
                if (!attackerRef.Packed.Unpack(out _, out var attackerEntity))
                    continue;

                if (!playerTeamTagPool.Value.Has(attackerEntity))
                    continue;

                ref var originRef = ref heroInstanceOriginRefPool.Value.Get(attackerEntity);
                if (!originRef.Packed.Unpack(out var origWorld, out var origEntity))
                    continue;

                var scoreRefPool = origWorld.GetPool<RelationScoreRef>();

                if (!scoreRefPool.Has(origEntity))
                    continue; // battle without raid

                ref var scoreRef = ref scoreRefPool.Get(origEntity); 
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(attackerEntity);

                foreach (var party in scoreRef.Parties)
                    TryCastRelationEffect(origWorld, heroConfigRef.Packed, party.Key, party.Value);

            }
        }

        /// <summary>
        /// Will add relation effect if rules applied will allow to
        /// </summary>
        /// <param name="origWorld">EcsWorld to take relations from</param>
        /// <param name="heroConfigPackedEntity">Hero Config Entity of the given hero</param>
        /// <param name="otherGuyPacked">Packed other guy entity in the origin world</param>
        /// <param name="scoreEntityPacked">Packed score entity for the given hero and the other guy</param>
        private void TryCastRelationEffect(
            EcsWorld origWorld,
            EcsPackedEntityWithWorld heroConfigPackedEntity,
            EcsPackedEntity otherGuyPacked, 
            EcsPackedEntity scoreEntityPacked)
        {
            if (!otherGuyPacked.Unpack(origWorld, out var otherGuyEntity))
                throw new Exception("Stale Other Guy Entity (probably dead already)");

            if (!scoreEntityPacked.Unpack(origWorld, out var scoreEntity))
                throw new Exception("Stale Score Entity");
            
            var relationsConfig = heroLibraryService.Value.HeroRelationsConfigProcessor();

            var score = origWorld.ReadIntValue<RelationScoreTag>(scoreEntity);
            var relationsState = relationsConfig.GetRelationState(score);

            
            if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero Config for current guy");

            var libHeroPool = libWorld.GetPool<Hero>();
            ref var heroConfig = ref libHeroPool.Get(heroConfigEntity);

            var rulesCaseKey = new RelationEffectLibraryKey(
                heroConfig.Id, RelationSubjectState.Attacking, relationsState);

            var effectRules = heroLibraryService.Value.HeroRelationEffectsLibProcessor();

            if (!effectRules.SubjectStateEffectsIndex.TryGetValue(rulesCaseKey, out var scope))
                return; // no effect for relation state, it's ok

            var origWorldConfigRefPool = origWorld.GetPool<HeroConfigRefComp>();
            ref var otherGuyConfigRef = ref origWorldConfigRefPool.Get(otherGuyEntity);
            if (!otherGuyConfigRef.Packed.Unpack(out _, out var otherGuyHeroConfigEntity))
                throw new Exception("No Hero Config for the other guy");

            ref var otherGuyHeroConfig = ref libHeroPool.Get(otherGuyHeroConfigEntity);

            var rule = scope.EffectRule;
            var ruleType = rule.EffectType;

            Debug.Log($"Relations Effect of type {ruleType} was just spawned " +
                $"for {heroConfig.Name} in {scope.SelfState} due to {scope.RelationState} " +
                $"with {otherGuyHeroConfig.Name}");

            // TODO: respect spawn rate from AdditioinalEffectSpawnRate:
            // 1. add component to the entity of the current score data;
            // 2. keep all spawned effects (EffectRules) in that component;
            // 3. count of already spawned effects used as a wheight for spawn rate (key for 
            // AdditioinalEffectSpawnRate queries);

        }
    }
}
