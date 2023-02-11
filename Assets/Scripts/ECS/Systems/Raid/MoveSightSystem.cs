using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class MoveSightSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<VisitCellComp> visitPool;
        private readonly EcsPoolInject<SightRangeComp> sightRangePool;

        private readonly EcsFilterInject<Inc<SightRangeComp, FieldCellComp, VisitCellComp>> visitFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);
                var oldCellId = cellComp.CellIndex;

                ref var visitComp = ref visitPool.Value.Get(entity);
                var nextCellId = visitComp.CellIndex;

                ref var sightRange = ref sightRangePool.Value.Get(entity);

                worldService.Value.UpdateVisibilityInRange(oldCellId, nextCellId, sightRange.Range);
            }
        }
    }
}