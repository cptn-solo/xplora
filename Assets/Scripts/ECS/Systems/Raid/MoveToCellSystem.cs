using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class MoveToCellSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;
        private readonly EcsPoolInject<VisitCellComp> visitPool = default;
        private readonly EcsPoolInject<DrainComp> drainPool = default;
        private readonly EcsPoolInject<BuffComp<NoStaminaDrainBuffTag>> staminaBuffPool = default;

        private readonly EcsFilterInject<Inc<FieldCellComp, VisitCellComp>> visitFilter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;
        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);
                var oldCellId = cellComp.CellIndex;

                ref var visitComp = ref visitPool.Value.Get(entity);
                var nextCellId = visitComp.CellIndex;

                if (staminaBuffPool.Value.Has(entity))
                {
                    ref var buffComp = ref staminaBuffPool.Value.Get(entity);
                    if (buffComp.Usages > 1)
                        buffComp.Usages--;
                    else
                        staminaBuffPool.Value.Del(entity);
                }
                else
                {
                    if (!drainPool.Value.Has(entity))
                        drainPool.Value.Add(entity);

                    ref var drainComp = ref drainPool.Value.Get(entity);
                    drainComp.Value += 10;
                }                

                cellComp.CellIndex = nextCellId;

                worldService.Value.VisitWorldCell(oldCellId, nextCellId, raidService.Value.PlayerEntity);
            }
        }
    }
}