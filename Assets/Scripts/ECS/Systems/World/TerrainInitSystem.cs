using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<WorldComp> worldPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            if (!worldService.Value.WorldEntity.Unpack(out var world, out var entity))
                return;

            ref var worldComp = ref worldPool.Value.Get(entity);

            for (int i = 0; i < worldComp.CellPackedEntities.Length; i++)
            {
                var cellEntity = world.NewEntity();
                ref var cellComp = ref cellPool.Value.Add(cellEntity);
                cellComp.CellIndex = i;

                worldComp.CellPackedEntities[i] = world.PackEntity(cellEntity);
            }
        }
    }
}