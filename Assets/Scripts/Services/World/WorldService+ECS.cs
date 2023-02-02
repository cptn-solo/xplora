using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class WorldService // ECS
    {
        public EcsPackedEntity WorldEntity { get; set; }

        private EcsWorld ecsWorld;
        private IEcsSystems ecsSystems;

        internal EcsPackedEntity[] cellPackedEntities;

        private void StartEcsWorldContext()
        {
            cellPackedEntities = new EcsPackedEntity[CellCount];

            ecsWorld = new EcsWorld();
            ecsSystems = new EcsSystems(ecsWorld);

            ecsSystems
                .Add(new WorldInitSystem())
                .Add(new TerrainGenerationSystem())
                .Add(new TerrainDestructionSystem())
                .Add(new GarbageCollectorSystem())
#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();
        }

        private void StopEcsWorldContext()
        {
            ecsSystems?.Destroy();
            ecsSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }

        private void ProduceEcsWorld()
        {
            if (!WorldEntity.Unpack(ecsWorld, out var worldEntity))
                return;

            ecsWorld.GetPool<ProduceTag>().Add(worldEntity);
        }

        internal EcsPackedEntity CellPackedEntity(int cellId)
        {
            return cellPackedEntities[cellId];
        }

        private void DestroyEcsWorld()
        {
            if (!WorldEntity.Unpack(ecsWorld, out var worldEntity))
                return;

            ecsWorld.GetPool<DestroyTag>().Add(worldEntity);

        }

        internal void AddPoi<T>(int cellIndex, EcsPackedEntityWithWorld ecsPackedEntity)
            where T: struct
        {
            if (cellPackedEntities.Length <= cellIndex ||
                !cellPackedEntities[cellIndex].Unpack(ecsWorld, out var cellEntity))
                return;

            ref var poiComp = ref ecsWorld.GetPool<POIComp>().Add(cellEntity);
            poiComp.PackedEntity = ecsPackedEntity;

            ecsWorld.GetPool<T>().Add(cellEntity);
        }

        internal void DeletePoi<T>(int cellIndex)
            where T: struct
        {
            if (cellPackedEntities.Length <= cellIndex ||
                !cellPackedEntities[cellIndex].Unpack(ecsWorld, out var cellEntity))
                return;

            var poiPool = ecsWorld.GetPool<POIComp>();

            if (!poiPool.Has(cellEntity) ||
                !ecsWorld.GetPool<T>().Has(cellEntity))
                return;

            poiPool.Del(cellEntity);

            var pool = ecsWorld.GetPool<T>();
            pool.Del(cellEntity);
        }

        internal bool TryGetPoi<T>(int cellIndex, out EcsPackedEntityWithWorld poiPackedEntity)
            where T: struct
        {
            poiPackedEntity = default;

            if (cellPackedEntities.Length <= cellIndex ||
                !cellPackedEntities[cellIndex].Unpack(ecsWorld, out var cellEntity))
                return false;

            var poiPool = ecsWorld.GetPool<POIComp>();

            if (!poiPool.Has(cellEntity) ||
                !ecsWorld.GetPool<T>().Has(cellEntity))
                return false;

            ref var poiComp = ref poiPool.Get(cellEntity);
            poiPackedEntity = poiComp.PackedEntity;

            return true;
        }
    }

}

