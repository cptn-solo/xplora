using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class OutOfSightSystem : IEcsRunSystem
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
                if (!cellPool.Value.Has(entity))
                    continue;

                ref var cellComp = ref cellPool.Value.Get(entity);
                var oldCellId = cellComp.CellIndex;

                ref var visitComp = ref visitPool.Value.Get(entity);
                var nextCellId = visitComp.CellIndex;

                ref var sightRange = ref sightRangePool.Value.Get(entity);

                if (nextCellId != oldCellId)
                    worldService.Value.DecreaseVisibilityInRange(oldCellId, sightRange.Range);
            }
        }
    }
}