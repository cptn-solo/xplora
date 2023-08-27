using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleEnqueueTurnSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<DraftTag> draftTagPool = default;
        private readonly EcsPoolInject<SceneVisualsQueueComp> turnVisualsPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo>> turnInfoFilter = default;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            if (turnInfoFilter.Value.GetEntitiesCount() == 0)
                PrepareEcsNextTurn();
        }
        private void PrepareEcsNextTurn()
        {
            var entity = ecsWorld.Value.NewEntity();

            ref var turnInfo = ref turnInfoPool.Value.Add(entity);
            draftTagPool.Value.Add(entity);
            ref var turnVisuals = ref turnVisualsPool.Value.Add(entity);
            turnVisuals.QueuedVisuals = new List<EcsPackedEntity>();
        }

    }
}
