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

        private EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();
            ref var worldComp = ref worldPool.Value.Add(entity);
            worldComp.CellPackedEntities =
                new EcsPackedEntity[worldService.Value.CellCount];

            worldService.Value.SetWorldEntity(ecsWorld.Value.PackEntityWithWorld(entity));

            for (int i = 0; i < worldService.Value.CellCount; i++)
            {
                var cellEntity = ecsWorld.Value.NewEntity();
                ref var cellComp = ref cellPool.Value.Add(cellEntity);
                cellComp.CellIndex = i;

                worldComp.CellPackedEntities[i] = ecsWorld.Value.PackEntity(cellEntity);
            }

            worldComp.PowerSourceCount = (int)Mathf.Pow(
                Mathf.Sqrt((int)worldService.Value.CellCount) * .1f, 2);

        }
    }
}