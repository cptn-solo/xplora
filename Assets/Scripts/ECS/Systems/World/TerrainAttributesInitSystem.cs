using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainAttributesInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<WorldComp> worldPool;
        private readonly EcsPoolInject<TerrainAttributeComp> attributePool;

        private readonly EcsFilterInject<Inc<FieldCellComp>, Exc<NonPassableTag>> accessibleFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            if (!worldService.Value.WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            ref var worldComp = ref worldPool.Value.Get(worldEntity);
            var initiallyAccessibleCellCount = accessibleFilter.Value.GetEntitiesCount();

            foreach (var attribute in worldService.Value.TerrainAttributesLibrary.TerrainAttributes)
            {
                if (attribute.Value.SpawnRate <= 0)
                    continue;

                var count = (int) (initiallyAccessibleCellCount * attribute.Value.SpawnRate * .01);
                int[] freeCellsIndexes = worldService.Value
                    .GetRandomFreeCellIndexes(count);

                for (var idx = 0; idx < freeCellsIndexes.Length; idx++)
                {
                    var cellIndex = freeCellsIndexes[idx];

                    if (!worldComp.CellPackedEntities[cellIndex].Unpack(world, out var cellEntity))
                        continue;

                    ref var attributeComp = ref attributePool.Value.Add(cellEntity);
                    attributeComp.TerrainAttribute = attribute.Key;
                }
            }



        }
    }
}