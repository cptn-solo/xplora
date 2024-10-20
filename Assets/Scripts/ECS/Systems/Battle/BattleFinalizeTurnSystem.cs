﻿using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleFinalizeTurnSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<GarbageTag> garbageTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<ProcessedHeroTag> processedHeroPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<FinalizedTurnTag> finalizedPool = default;
        
        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, ProcessedTurnTag>,
            Exc<FinalizedTurnTag>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                FinalizeTurn(entity);
                finalizedPool.Value.Add(entity);
            }
        }

        private void FinalizeTurn(int turnEntity)
        {

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            turnInfo.State = TurnState.TurnProcessed;
            battleService.Value.NotifyTurnEventListeners(turnInfo);

            if (attackerRefPool.Value.Has(turnEntity))
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
                if (attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerInstanceEntity) &&
                    !processedHeroPool.Value.Has(attackerInstanceEntity))
                    processedHeroPool.Value.Add(attackerInstanceEntity);
            }

            if (targetRefPool.Value.Has(turnEntity))
            {
                ref var targetRef = ref targetRefPool.Value.Get(turnEntity);
                if (targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetInstanceEntity) &&
                    !processedHeroPool.Value.Has(targetInstanceEntity))
                    processedHeroPool.Value.Add(targetInstanceEntity);
            }

            if (!battleService.Value.RoundEntity.Unpack(out var world, out var roundEntity))
                throw new Exception("No round");

            ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);

            var buffer = ListPool<RoundSlotInfo>.Get();

            buffer.AddRange(roundInfo.QueuedHeroes);

            if (buffer.Count > 0)
                buffer.RemoveAt(0);

            roundInfo.QueuedHeroes = buffer.ToArray();

            ListPool<RoundSlotInfo>.Add(buffer);

            garbageTagPool.Value.Add(turnEntity);
        }

    }
}
