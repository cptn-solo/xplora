using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignAttackerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnPool;
        private readonly EcsPoolInject<BattleRoundInfo> roundPool;
        private readonly EcsPoolInject<HeroInstanceRefComp> heroInstanceRefPool;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool;

        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                AssignAttacker(entity);
        }

        internal void AssignAttacker(int turnEntity)
        {
            if (!battleService.Value.RoundEntity.Unpack(out var world, out var roundEntity))
                throw new Exception("No Round");

            ref var roundInfo = ref roundPool.Value.Get(roundEntity);
            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            
            var roundSlot = roundInfo.QueuedHeroes[0];

            if (!roundSlot.HeroInstancePackedEntity.Unpack(out _, out var attackerInstanceEntity))
                throw new Exception("No Hero instance");

            ref var attackerConfigRef = ref heroConfigRefPool.Value.Get(attackerInstanceEntity);

            if (!attackerConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero Config");

            var heroConfigPool = libWorld.GetPool<Hero>();
            ref var attackerConfig = ref heroConfigPool.Get(heroConfigEntity);
            turnInfo.Attacker = attackerConfig;

            ref var attackerRef = ref attackerRefPool.Value.Add(turnEntity);
            attackerRef.HeroInstancePackedEntity = world.PackEntityWithWorld(attackerInstanceEntity);
        }
    }
}
