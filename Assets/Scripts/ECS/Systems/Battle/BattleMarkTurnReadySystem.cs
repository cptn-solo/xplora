using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleMarkTurnReadySystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<ReadyTurnTag> readyTurnTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<SkippedTag> skippedTagPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, DraftTag>, Exc<ReadyTurnTag>> filter = default;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            foreach (var entity in filter.Value)
                ProcessTurn(entity);           
        }

        private bool ProcessTurn(int entity)
        {
            ref var turnInfo = ref turnInfoPool.Value.Get(entity);

            if (skippedTagPool.Value.Has(entity))
                return MarkTurnReady(entity, turnInfo);

            return turnInfo.State switch
            {
                TurnState.TurnPrepared or TurnState.NoTargets => MarkTurnReady(entity, turnInfo),
                _ => false,
            };
        }

        private bool MarkTurnReady(int entity, BattleTurnInfo turnInfo)
        {
            readyTurnTagPool.Value.Add(entity);
            battleService.Value.NotifyTurnEventListeners(turnInfo);
            return true;
        }
    }
}
