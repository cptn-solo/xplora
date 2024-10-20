﻿using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueCompletedRoundSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<RoundShortageTag> roundShortageTagPool = default;

        private readonly EcsFilterInject<Inc<BattleRoundInfo, GarbageTag>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                DequeueRound(entity);
        }

        private void DequeueRound(int roundEntity)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                throw new Exception("No battle");

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            var buffer = ListPool<EcsPackedEntity>.Get();

            buffer.AddRange(battleInfo.QueuedRounds);
            if (buffer.Count > 0)
                buffer.RemoveAt(0);

            battleInfo.QueuedRounds = buffer.ToArray();

            ListPool<EcsPackedEntity>.Add(buffer);

            if (battleInfo.QueuedRounds.Length > 0 &&
                battleInfo.QueuedRounds[0].Unpack(world, out roundEntity))
                battleService.Value.RoundEntity = world.PackEntityWithWorld(roundEntity);

            if (!roundShortageTagPool.Value.Has(battleEntity))
                roundShortageTagPool.Value.Add(battleEntity);
        }
    }
}
