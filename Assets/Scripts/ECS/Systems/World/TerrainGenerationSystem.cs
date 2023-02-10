using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainGenerationSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<WorldComp> worldPool;
        private readonly EcsPoolInject<FieldVisibilityRef> visibilityRefPool;

        private readonly EcsFilterInject<Inc<WorldComp, ProduceTag>> worlFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var worldEntity in worlFilter.Value)
            {
                ref var worldComp = ref worldPool.Value.Get(worldEntity);
                var packed = worldComp.CellPackedEntities;

                CellProducerCallback cellCallback = (IVisibility cell, int index) => {
                    if (!packed[index].Unpack(ecsWorld.Value, out var cellEntity))
                        return;

                    ref var visibilityRef = ref visibilityRefPool.Value.Add(cellEntity);
                    visibilityRef.visibility = cell;

                    cell.ResetVisibility();
                };

                worldService.Value.GenerateTerrain(cellCallback);
            }
        }
    }
}