using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainGenerationSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<WorldComp> worldPool = default;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool = default;

        private readonly EcsPoolInject<ExploredTag> exploredTagPool = default;
        private readonly EcsPoolInject<TerrainTypeComp> terrainTypePool = default;

        private readonly EcsFilterInject<Inc<WorldComp, ProduceTag>> worlFilter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public override void RunIfActive(IEcsSystems systems)
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

                    ref var terrainType = ref terrainTypePool.Value.Get(cellEntity);

                    cell.ResetVisibility();
                    cell.Load(terrainType.TerrainType, exploredTagPool.Value.Has(cellEntity));
                };

                worldService.Value.GenerateTerrain(cellCallback);
            }
        }
    }
}