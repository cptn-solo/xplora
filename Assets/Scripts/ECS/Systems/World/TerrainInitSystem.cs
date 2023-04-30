using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<WorldComp> worldPool = default;
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

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