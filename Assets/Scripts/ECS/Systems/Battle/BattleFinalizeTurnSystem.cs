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
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, ProcessedTurnTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                FinalizeTurn(entity);
            }
        }

        private void FinalizeTurn(int turnEntity)
        {
            var buffer = ListPool<RoundSlotInfo>.Get();

            if (!battleService.Value.RoundEntity.Unpack(out var world, out var roundEntity))
                throw new Exception("No round");

            ref var round = ref roundInfoPool.Value.Get(roundEntity);

            buffer.AddRange(round.QueuedHeroes);

            if (buffer.Count > 0)
                buffer.RemoveAt(0);

            round.QueuedHeroes = buffer.ToArray();
            if (round.QueuedHeroes.Length == 0)
            {
                world.GetPool<DestroyTag>().Add(roundEntity);
            }

            ListPool<RoundSlotInfo>.Add(buffer);

            world.GetPool<DestroyTag>().Add(turnEntity);
        }

    }
}
