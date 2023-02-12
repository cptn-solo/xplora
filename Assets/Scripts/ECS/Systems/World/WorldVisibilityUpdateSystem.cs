using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldVisibilityUpdateSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool;
        private readonly EcsPoolInject<VisibleTag> visibleTagPool;

        private readonly EcsFilterInject<
            Inc<VisibilityUpdateTag, FieldCellComp, VisibilityRef>> filter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);

                ref var visibilityRef = ref visibilityRefPool.Value.Get(entity);
                var visible = visibleTagPool.Value.Has(entity);

                worldService.Value.CellVisibilityUpdated(
                    cellComp.CellIndex, visibilityRef.visibility, visible);
            }
        }
    }
}