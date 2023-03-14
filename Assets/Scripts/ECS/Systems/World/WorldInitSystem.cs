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

            worldComp.CellPackedEntities =
                new EcsPackedEntity[worldService.Value.CellCount];

            var cellCount = worldService.Value.CellCount;

            worldComp.PowerSourceCount =
                (int)(cellCount * Random.Range(.25f, .5f) / 100);

            worldComp.HPSourceCount =
                (int)(cellCount * Random.Range(.25f, .5f) / 100);

            worldComp.WatchTowerCount =
                (int)(cellCount * Random.Range(.05f, .1f) / 100);

            worldService.Value.SetWorldEntity(ecsWorld.Value.PackEntityWithWorld(entity));
        }
    }
}