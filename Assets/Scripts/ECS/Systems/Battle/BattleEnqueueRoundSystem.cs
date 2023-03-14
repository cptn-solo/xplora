using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleEnqueueRoundSystem : IEcsRunSystem
    {
        private const int minRoundsQueue = 4;

        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<RoundShortageTag> roundShortageTagPool = default;
        private readonly EcsPoolInject<DraftTag> draftTagPool = default;
        
        private readonly EcsFilterInject<
            Inc<BattleInfo, RoundShortageTag>,
            Exc<BattleCompletedTag>> battleFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in battleFilter.Value)
            {
                EnqueueEcsRound(entity);
            }
        }

        private void EnqueueEcsRound(int battleEntity)
        {
            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            if (battleInfo.QueuedRounds.Length > minRoundsQueue)
                throw new Exception("Rounds queue length exceeded");

            var roundEntity = ecsWorld.Value.NewEntity();

            ref var roundInfo = ref roundInfoPool.Value.Add(roundEntity);
            roundInfo.Round = ++battleInfo.LastRoundNumber;

            draftTagPool.Value.Add(roundEntity);
            
            battleInfo.LastRoundNumber = roundInfo.Round;

            var buffer = ListPool<EcsPackedEntity>.Get();

            buffer.AddRange(battleInfo.QueuedRounds);
            buffer.Add(ecsWorld.Value.PackEntity(roundEntity));
            battleInfo.QueuedRounds = buffer.ToArray();

            ListPool<EcsPackedEntity>.Add(buffer);

            if (battleInfo.QueuedRounds[0].Unpack(ecsWorld.Value, out var currentRoundEntity))
                battleService.Value.RoundEntity = ecsWorld.Value.PackEntityWithWorld(currentRoundEntity);

            if (battleInfo.QueuedRounds.Length >= minRoundsQueue)
                roundShortageTagPool.Value.Del(battleEntity);

        }
    }
}
