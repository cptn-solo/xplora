using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class InSightSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<VisitCellComp> visitPool;
        private readonly EcsPoolInject<SightRangeComp> sightRangePool;

        private readonly EcsFilterInject<Inc<SightRangeComp, VisitCellComp>> visitFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var visitComp = ref visitPool.Value.Get(entity);
                ref var sightRange = ref sightRangePool.Value.Get(entity);

                worldService.Value.IncreaseVisibilityInRange(visitComp.CellIndex, sightRange.Range);
            }
        }
    }
}