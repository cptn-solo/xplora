using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleMarkTurnReadySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ReadyTurnTag> readyTurnTagPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, DraftTag>, Exc<ReadyTurnTag>> filter;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            foreach (var entity in filter.Value)
            {
                ref var turnInfo = ref turnInfoPool.Value.Get(entity);
                switch (turnInfo.State)
                {
                    case TurnState.TurnPrepared:
                    case TurnState.TurnSkipped:
                    case TurnState.NoTargets:
                        {
                            readyTurnTagPool.Value.Add(entity);
                            battleService.Value.NotifyTurnEventListeners(turnInfo);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
