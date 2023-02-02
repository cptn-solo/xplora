using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RemoveWorldPoiSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<DestroyTag> garbagePool;

        private readonly EcsFilterInject<Inc<OpponentComp, RetireTag>> opponentToRetireFilter;

        private readonly EcsCustomInject<WorldService> worldService;

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