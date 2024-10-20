﻿using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignAttackerSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;

        private readonly EcsPoolInject<HeroConfigRef> heroConfigRefPool = default;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                AssignAttacker(entity);
        }

        internal void AssignAttacker(int turnEntity)
        {
            if (!battleService.Value.RoundEntity.Unpack(out _, out var roundEntity))
                throw new Exception("No Round");

            ref var roundInfo = ref roundPool.Value.Get(roundEntity);
            if (roundInfo.QueuedHeroes.Length == 0)
                throw new Exception("Round Queue empty");

            var roundSlot = roundInfo.QueuedHeroes[0];

            if (!roundSlot.HeroInstancePackedEntity.Unpack(out _, out var attackerInstanceEntity))
                throw new Exception("No Hero instance");

            ref var attackerConfigRef = ref heroConfigRefPool.Value.Get(attackerInstanceEntity);

            if (!attackerConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero Config");

            var heroConfigPool = libWorld.GetPool<Hero>();
            ref var attackerConfig = ref heroConfigPool.Get(heroConfigEntity);
            Debug.Log($"Attacker: {attackerConfig.Name}");

            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            turnInfo.AttackerConfig = attackerConfig;
            turnInfo.Attacker = roundSlot.HeroInstancePackedEntity;

            ref var attackerRef = ref attackerRefPool.Value.Add(turnEntity);
            attackerRef.HeroInstancePackedEntity = roundSlot.HeroInstancePackedEntity;
        }
    }
}
