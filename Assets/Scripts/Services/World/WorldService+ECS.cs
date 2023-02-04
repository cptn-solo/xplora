﻿using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public partial class WorldService // ECS
    {
        private EcsWorld ecsWorld;
        private IEcsSystems ecsSystems;

        internal EcsPackedEntityWithWorld WorldEntity { get; private set; }

        internal void SetWorldEntity(EcsPackedEntityWithWorld entity) =>
            WorldEntity = entity;

        private void StartEcsWorldContext()
        {
            ecsWorld = new EcsWorld();
            ecsSystems = new EcsSystems(ecsWorld);

            ecsSystems
                .Add(new WorldInitSystem())
                .Add(new WorldPowerSourceInitSystem())
                .Add(new WorldPoiInitSystem())
                .Add(new TerrainGenerationSystem())
                .Add(new DeployPoiSystem())
                .Add(new DestroyPoiSystem())
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
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            world.GetPool<ProduceTag>().Add(worldEntity);
        }

        /// <summary>
        /// Enqueues deployment only of the poi generated by the world, not raid poi.
        /// Raid deploys it's poi by itself
        /// </summary>
        private void DeployEcsWorldPoi()
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            var produceTagPool = world.GetPool<ProduceTag>();
            var positionedPoiFilter = world
                .Filter<POIComp>()
                .Inc<WorldPoiTag>()
                .Inc<FieldCellComp>()
                .End();

            foreach (var entity in positionedPoiFilter)
                produceTagPool.Add(entity);
        }

        private void DestroyEcsWorldPoi()
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            var destroyTagPool = world.GetPool<DestroyTag>();
            var deployedPoiFilter = world
                .Filter<WorldPoiTag>()
                .Inc<PoiRefComp>()
                .End();

            foreach (var entity in deployedPoiFilter)
                destroyTagPool.Add(entity);
        }

        /// <summary>
        /// Adds world reference to POI entity with world it belongs to
        /// </summary>
        /// <typeparam name="T">Component to assign in the world
        /// (defines type of the POI being added)</typeparam>
        /// <param name="cellIndex">Where to place</param>
        /// <param name="ecsPackedEntity">Entity with world</param>
        internal void AddPoi<T>(int cellIndex, EcsPackedEntityWithWorld ecsPackedEntity)
            where T: struct
        {
            if (!TryGetCellEntity(cellIndex, out var cellEntity, out var world))
                return;

            ref var poiComp = ref world.GetPool<POIComp>().Add(cellEntity);
            ref var packedRef = ref world.GetPool<PackedEntityRef>().Add(cellEntity);
            packedRef.PackedEntity = ecsPackedEntity;

            world.GetPool<T>().Add(cellEntity);
        }

        /// <summary>
        /// Deletes POI of specified type from the wolrd cell
        /// </summary>
        /// <typeparam name="T">Component to remove from cellentity
        /// (POI component will be removed too)</typeparam>
        /// <param name="cellIndex"></param>
        internal void DeletePoi<T>(int cellIndex)
            where T: struct
        {
            if (!TryGetCellEntity(cellIndex, out var cellEntity, out var world))
                return;

            var poiPool = world.GetPool<POIComp>();
            var packedRefPool = world.GetPool<PackedEntityRef>();

            if (!poiPool.Has(cellEntity) ||
                !packedRefPool.Has(cellEntity) ||
                !world.GetPool<T>().Has(cellEntity))
                return;

            poiPool.Del(cellEntity);

            var pool = world.GetPool<T>();
            pool.Del(cellEntity);
        }
        /// <summary>
        /// Tries to get POI of the specified type T
        /// </summary>
        /// <typeparam name="T">Component to look for (in addition to POI itself)</typeparam>
        /// <param name="cellIndex"></param>
        /// <param name="poiPackedEntity">Will carry out the entity of
        /// the POI in the world it belongs to</param>
        /// <returns></returns>
        internal bool TryGetPoi<T>(int cellIndex, out EcsPackedEntityWithWorld poiPackedEntity)
            where T: struct
        {
            poiPackedEntity = default;

            if (!TryGetCellEntity(cellIndex, out var cellEntity, out var world))
                return false;

            var poiPool = world.GetPool<POIComp>();
            var packedRefPool = world.GetPool<PackedEntityRef>();

            if (!poiPool.Has(cellEntity) ||
                !packedRefPool.Has(cellEntity) ||
                !world.GetPool<T>().Has(cellEntity))
                return false;

            ref var poiComp = ref poiPool.Get(cellEntity);

            ref var packedRef = ref packedRefPool.Get(cellEntity);
            poiPackedEntity = packedRef.PackedEntity;

            return true;
        }

        private bool TryGetCellEntity(int cellIndex, out int entity, out EcsWorld world)
        {
            entity = -1;
            world = null;

            if (!WorldEntity.Unpack(out world, out var worldEntity))
                return false;

            var worldPool = world.GetPool<WorldComp>();
            ref var worldComp = ref worldPool.Get(worldEntity);

            if (worldComp.CellPackedEntities.Length <= cellIndex ||
                !worldComp.CellPackedEntities[cellIndex].Unpack(world, out entity))
                return false;

            return true;

        }

        /// <summary>
        /// Gets free indexes, but does the scan only for cells with POIComp on them
        /// so player is not visible by this method (he doesn't have POI reference)
        /// </summary>
        /// <param name="count">How many free cells we need</param>
        /// <returns></returns>
        internal int[] GetRandomFreeCellIndexes(int count)
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return new int[] { };

            var freeCellFilter = world.Filter<FieldCellComp>().Exc<POIComp>().End();
            var cellPool = world.GetPool<FieldCellComp>();
            var cellCount = freeCellFilter.GetEntitiesCount();
            var sCount = count;
            var sLength = cellCount / sCount;

            List<int> segments = new();
            int[] freeIndexes = new int[sCount];

            for (int i = 0; i < sCount; i++)
                segments.Add(i);

            var en = freeCellFilter.GetRawEntities();
            for (int i = 0; i < sCount; i++)
            {
                var freeIndex = PickIndex();
                var freeCellEntity = en[freeIndex];
                ref var cellComp = ref cellPool.Get(freeCellEntity);
                freeIndexes[i] = cellComp.CellIndex;
            }

            int PickIndex()
            {
                var idx = Random.Range(0, segments.Count);
                var num = segments[idx];
                segments.RemoveAt(idx);

                var cIdx = Random.Range(sLength * num, sLength * (num + 1));

                return cIdx;
            }
            return freeIndexes;
        }
    }

}

