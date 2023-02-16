using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<WorldComp> worldPool;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();

            ref var worldComp = ref worldPool.Value.Add(entity);

            worldComp.CellPackedEntities =
                new EcsPackedEntity[worldService.Value.CellCount];

            worldComp.PowerSourceCount = (int)Mathf.Pow(
                Mathf.Sqrt((int)worldService.Value.CellCount) * .1f, 2);

            worldService.Value.SetWorldEntity(ecsWorld.Value.PackEntityWithWorld(entity));
        }
    }
}