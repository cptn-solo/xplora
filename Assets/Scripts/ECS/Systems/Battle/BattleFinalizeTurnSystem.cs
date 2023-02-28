using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleFinalizeTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<GarbageTag> garbageTagPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool;
        private readonly EcsPoolInject<ProcessedHeroTag> processedHeroPool;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<TargetRef> targetRefPool;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, ProcessedTurnTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                FinalizeTurn(entity);
        }

        private void FinalizeTurn(int turnEntity)
        {

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            turnInfo.State = TurnState.TurnProcessed;
            battleService.Value.NotifyTurnEventListeners(turnInfo);

            if (attackerRefPool.Value.Has(turnEntity))
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
                if (attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerInstanceEntity))
                    processedHeroPool.Value.Add(attackerInstanceEntity);
            }

            if (targetRefPool.Value.Has(turnEntity))
            {
                ref var targetRef = ref targetRefPool.Value.Get(turnEntity);
                if (targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetInstanceEntity))
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
