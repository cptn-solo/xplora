using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldInitSystem : IEcsInitSystem
    {
        private EcsWorldInject ecsWorld;

        private EcsPoolInject<WorldComp> worldPool;
        private EcsPoolInject<FieldCellComp> cellPool;

        private EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();
            worldPool.Value.Add(entity);

            worldService.Value.WorldEntity = ecsWorld.Value.PackEntity(entity);

            for (int i = 0; i < worldService.Value.CellCount; i++)
            {
                var cellEntity = ecsWorld.Value.NewEntity();
                ref var cellComp = ref cellPool.Value.Add(cellEntity);
                cellComp.CellIndex = i;

                worldService.Value.cellPackedEntities[i] = ecsWorld.Value.PackEntity(cellEntity);
            }

        }
    }
}