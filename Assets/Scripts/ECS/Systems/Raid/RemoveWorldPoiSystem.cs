using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RemoveWorldPoiSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;
        private readonly EcsPoolInject<GarbageTag> garbagePool = default;

        private readonly EcsFilterInject<Inc<OpponentComp, RetireTag>> opponentToRetireFilter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in opponentToRetireFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);
                worldService.Value.DeletePoi<OpponentComp>(cellComp.CellIndex);

                if (!garbagePool.Value.Has(entity))
                    garbagePool.Value.Add(entity);

            }
        }
    }
}