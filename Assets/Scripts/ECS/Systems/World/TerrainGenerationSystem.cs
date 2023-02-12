using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainGenerationSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<WorldComp> worldPool;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool;

        private readonly EcsPoolInject<ExploredTag> exploredTagPool;
        private readonly EcsPoolInject<TerrainTypeComp> terrainTypePool;

        private readonly EcsFilterInject<Inc<WorldComp, ProduceTag>> worlFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var worldEntity in worlFilter.Value)
            {
                ref var worldComp = ref worldPool.Value.Get(worldEntity);
                var packed = worldComp.CellPackedEntities;

                var terrainTypes = new TerrainType[2] {
                    TerrainType.Grass,
                    TerrainType.LightGrass,
                };

                CellProducerCallback cellCallback = (IVisibility cell, int index) => {
                    if (!packed[index].Unpack(ecsWorld.Value, out var cellEntity))
                        return;

                    ref var visibilityRef = ref visibilityRefPool.Value.Add(cellEntity);
                    visibilityRef.visibility = cell;

                    var tti = -1;
                    if (!terrainTypePool.Value.Has(cellEntity))
                    {
                        ref var terrainType = ref terrainTypePool.Value.Add(cellEntity);
                        terrainType.TerrainType = terrainTypes[Random.Range(0, terrainTypes.Length)];
                        tti = (int)terrainType.TerrainType;
                    }
                    else
                    {
                        ref var terrainType = ref terrainTypePool.Value.Get(cellEntity);
                        tti = (int)terrainType.TerrainType;
                    }

                    cell.ResetVisibility();
                    cell.Load(tti, exploredTagPool.Value.Has(cellEntity));
                };

                worldService.Value.GenerateTerrain(cellCallback);
            }
        }
    }
}