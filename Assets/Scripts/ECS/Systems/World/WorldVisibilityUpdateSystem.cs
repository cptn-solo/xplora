using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldVisibilityUpdateSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool = default;
        private readonly EcsPoolInject<VisibleTag> visibleTagPool = default;

        private readonly EcsFilterInject<
            Inc<VisibilityUpdateTag, FieldCellComp, VisibilityRef>> filter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public override void RunIfActive(IEcsSystems systems)
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