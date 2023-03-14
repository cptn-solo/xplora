using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleEnqueueTurnSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<DraftTag> draftTagPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo>> turnInfoFilter = default;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter = default;

        public void Run(IEcsSystems systems)
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
        }

    }
}
