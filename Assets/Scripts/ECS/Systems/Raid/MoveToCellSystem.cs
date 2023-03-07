using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class MoveToCellSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<VisitCellComp> visitPool;
        private readonly EcsPoolInject<DrainComp> drainPool;

        private readonly EcsFilterInject<Inc<FieldCellComp, VisitCellComp>> visitFilter;

        private readonly EcsCustomInject<WorldService> worldService;
        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);
                var oldCellId = cellComp.CellIndex;

                ref var visitComp = ref visitPool.Value.Get(entity);
                var nextCellId = visitComp.CellIndex;

                if (!drainPool.Value.Has(entity))
                    drainPool.Value.Add(entity);

                ref var drainComp = ref drainPool.Value.Get(entity);
                drainComp.Value += 10;

                worldService.Value.VisitWorldCell(oldCellId, nextCellId, raidService.Value.PlayerEntity);
            }
        }
    }
}