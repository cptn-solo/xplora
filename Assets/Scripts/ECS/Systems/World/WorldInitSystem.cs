using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldInitSystem : IEcsInitSystem
    {
        private EcsWorldInject ecsWorld;

        private EcsPoolInject<WorldComp> worldPool;
        private EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<NonPassableTag> noGoTagPool;
        private readonly EcsPoolInject<TerrainTypeComp> terrainTypePool;

        private EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();
            ref var worldComp = ref worldPool.Value.Add(entity);
            worldComp.CellPackedEntities =
                new EcsPackedEntity[worldService.Value.CellCount];

            worldService.Value.SetWorldEntity(ecsWorld.Value.PackEntityWithWorld(entity));

            var terrainTypes = new TerrainType[3] {
                    TerrainType.Grass,
                    TerrainType.LightGrass,
                    TerrainType.NoGo,
                };

            for (int i = 0; i < worldService.Value.CellCount; i++)
            {
                var cellEntity = ecsWorld.Value.NewEntity();
                ref var cellComp = ref cellPool.Value.Add(cellEntity);
                cellComp.CellIndex = i;

                ref var terrainType = ref terrainTypePool.Value.Add(cellEntity);
                terrainType.TerrainType = terrainTypes[Random.Range(0, terrainTypes.Length)];

                if (terrainType.TerrainType == TerrainType.NoGo)
                    noGoTagPool.Value.Add(cellEntity);

                worldComp.CellPackedEntities[i] = ecsWorld.Value.PackEntity(cellEntity);
            }

            worldComp.PowerSourceCount = (int)Mathf.Pow(
                Mathf.Sqrt((int)worldService.Value.CellCount) * .1f, 2);

        }
    }
}