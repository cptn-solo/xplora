using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRoundStartSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool;
        private readonly EcsPoolInject<RoundInProgressTag> roundInProgressTagPool;

        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleInfoFilter;
        private readonly EcsFilterInject<Inc<BattleRoundInfo, RoundInProgressTag>> roundInfoFilter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in battleInfoFilter.Value)
            {
                if (roundInfoFilter.Value.GetEntitiesCount() == 0)
                {
                    ref var battleInfo = ref battleInfoPool.Value.Get(entity);
                    if (battleInfo.QueuedRounds.Length > 0 &&
                        battleInfo.QueuedRounds[0].Unpack(ecsWorld.Value, out var roundEntity))
                    {
                        roundInProgressTagPool.Value.Add(roundEntity);

                        ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);
                        roundInfo.State = RoundState.RoundInProgress;
                        battleService.Value.NotifyRoundEventListeners(roundInfo);
                    }
                }
            }
        }
    }
}
