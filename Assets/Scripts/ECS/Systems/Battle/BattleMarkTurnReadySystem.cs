using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleMarkTurnReadySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ReadyTurnTag> readyTurnTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, DraftTag>, Exc<ReadyTurnTag>> filter = default;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

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
