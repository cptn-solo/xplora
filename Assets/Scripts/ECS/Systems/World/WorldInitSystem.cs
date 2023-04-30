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
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<WorldComp> worldPool = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();

            ref var worldComp = ref worldPool.Value.Add(entity);

            var config = worldService.Value.TerrainPOILibrary;

            worldComp.CellPackedEntities =
                new EcsPackedEntity[worldService.Value.CellCount];

            var cellCount = worldService.Value.CellCount;

            var psRate = config.SpawnRateForType(TerrainPOI.PowerSource);
            worldComp.PowerSourceCount =
                (int)(cellCount * Random.Range(psRate.MinRate, psRate.MaxRate) / 100);

            var hpsRate = config.SpawnRateForType(TerrainPOI.HPSource);
            worldComp.HPSourceCount =
                (int)(cellCount * Random.Range(hpsRate.MinRate, hpsRate.MaxRate) / 100);

            var wtRate = config.SpawnRateForType(TerrainPOI.WatchTower);
            worldComp.WatchTowerCount =
                (int)(cellCount * Random.Range(wtRate.MinRate, wtRate.MaxRate) / 100);

            worldService.Value.SetWorldEntity(ecsWorld.Value.PackEntityWithWorld(entity));
        }
    }
}