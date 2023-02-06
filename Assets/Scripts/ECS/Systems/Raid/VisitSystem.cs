using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Links cell to visitor
    /// </summary>
    public class VisitSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<VisitCellComp> visitPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<RefillComp> refillPool;
        private readonly EcsPoolInject<PoiRef> poiRefPool;

        private readonly EcsFilterInject<Inc<StaminaComp, VisitCellComp>> visitFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in visitFilter.Value)
            {
                ref var visitComp = ref visitPool.Value.Get(entity);
                var nextCellId = visitComp.CellIndex;

                if (!cellPool.Value.Has(entity))
                    cellPool.Value.Add(entity);

                ref var cellComp = ref cellPool.Value.Get(entity);
                cellComp.CellIndex = nextCellId;

                if (worldService.Value.TryGetPoi(
                    nextCellId, out var poiPackedEntity) &&
                    poiPackedEntity.Unpack(out var world, out var poiEntity))
                {
                    if (world.GetPool<PowerSourceComp>().Has(poiEntity) &&
                        !world.GetPool<OutOfPowerTag>().Has(poiEntity))
                    {
                        // refill for player event + drain for source event
                        ref var refillComp = ref refillPool.Value.Add(entity);
                        refillComp.Value = 10;

                        var drainPool = world.GetPool<DrainComp>();
                        ref var drainComp = ref drainPool.Add(poiEntity);
                        drainComp.Value = 10;
                    }
                    //TODO: battle can be moved here if initiated after the move, not before
                }
                else
                {
                    // drain for player event
                    var drainPool = ecsWorld.Value.GetPool<DrainComp>();

                    if (!drainPool.Has(entity))
                        drainPool.Add(entity);

                    ref var drainComp = ref drainPool.Get(entity);
                    drainComp.Value += 10;
                }
            }
        }
    }
}