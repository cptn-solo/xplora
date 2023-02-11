using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using UnityEngine;
using System.Collections.Generic;
using Leopotam.EcsLite.ExtendedSystems;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;

namespace Assets.Scripts.Services
{
    public partial class WorldService // ECS
    {
        private EcsWorld ecsWorld;
        private IEcsSystems ecsInitSystems;
        private IEcsSystems ecsSystems;

        internal EcsPackedEntityWithWorld WorldEntity { get; private set; }

        internal void SetWorldEntity(EcsPackedEntityWithWorld entity) =>
            WorldEntity = entity;

        private void StartEcsWorldContext()
        {
            ecsWorld = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsWorld);
            ecsInitSystems
                .Add(new WorldInitSystem())
                .Add(new WorldPowerSourceInitSystem())
                .Add(new WorldPoiInitSystem())
                .Inject(this)
                .Init();

            ecsSystems = new EcsSystems(ecsWorld);
            ecsSystems
                .Add(new TerrainGenerationSystem())
                .Add(new WorldVisibilityUpdateSystem())
                .Add(new WorldOutOfSightSystem())
                .Add(new WorldInSightSystem())
                .DelHere<VisibilityUpdateTag>()
                .Add(new DeployPoiSystem())
                .DelHere<ProduceTag>()
                .Add(new DrainSystem())
                .DelHere<DrainComp>()
                .Add(new UpdatePowerSourceSystem())
                .DelHere<UpdateTag>()
                .Add(new TerrainDestructionSystem())
                .Add(new DestroyPoiSystem())
                .DelHere<DestroyTag>()
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

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }

        private void ProduceEcsWorld()
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            world.GetPool<ProduceTag>().Add(worldEntity);
        }

        private void DestroyEcsWorld()
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            world.GetPool<DestroyTag>().Add(worldEntity);
        }
        
        private void DestroyEcsWorldPoi()
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            var destroyTagPool = world.GetPool<DestroyTag>();
            var deployedPoiFilter = world
                .Filter<WorldPoiTag>()
                .Inc<PoiRef>()
                .Exc<DestroyTag>()
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
            packedRefPool.Del(cellEntity);

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
        /// <returns>True if found POI combined with specified type component</returns>
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

        /// <summary>
        /// Tries to get Any POI
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <param name="poiPackedEntity">Will carry out the entity of
        /// the POI in the world it belongs to</param>
        /// <returns>True if POI comp found on the cell</returns>
        internal bool TryGetPoi(int cellIndex, out EcsPackedEntityWithWorld poiPackedEntity)
        {
            poiPackedEntity = default;

            if (!TryGetCellEntity(cellIndex, out var cellEntity, out var world))
                return false;

            var poiPool = world.GetPool<POIComp>();

            if (!poiPool.Has(cellEntity))
                return false;

            ref var poiComp = ref poiPool.Get(cellEntity);
            poiPackedEntity = world.PackEntityWithWorld(cellEntity);

            return true;
        }

        internal void UpdateVisibilityInRange(int prevCellIndex, int nextCellIndex, int range)
        {
            if (!TryGetCellEntity(nextCellIndex, out var cellEntity, out var world))
                return;

            var coordNext = CellCoordinatesResolver(nextCellIndex);
            var rangeCoordNext = coordNext.RangeFromCoordinates(range, new Vector4(width, height));

            var toHide = ListPool<HexCoordinates>.Get();
            var toShow = ListPool<HexCoordinates>.Get();

            toShow.AddRange(rangeCoordNext);

            if (prevCellIndex >= 0 &&
                prevCellIndex != nextCellIndex &&
                TryGetCellEntity(prevCellIndex, out var cellEntityPrev, out var _))
            {
                var coordPrev = CellCoordinatesResolver(prevCellIndex);
                var rangeCoordPrev = coordPrev.RangeFromCoordinates(range, new Vector4(width, height));

                toHide.AddRange(rangeCoordPrev);

                foreach (var check in rangeCoordNext)
                    toHide.Remove(check);

                foreach (var check in rangeCoordPrev)
                    toShow.Remove(check);
            }

            var visibilityUpdateTagPool = world.GetPool<VisibilityUpdateTag>();
            var visibilityRefPool = world.GetPool<VisibilityRef>();
            var visibleTagPool = world.GetPool<VisibleTag>();
            var exploredTagPool = world.GetPool<ExploredTag>();

            foreach (var refCoord in toHide)
            {
                var refIndex = CellIndexResolver(refCoord);

                if (!TryGetCellEntity(refIndex, out var refCellEntity, out var _))
                    continue;

                if (!visibilityRefPool.Has(refCellEntity))
                    continue;

                ref var visibilityRef = ref visibilityRefPool.Get(refCellEntity);
                visibilityRef.visibility.DecreaseVisibility();

                if (visibleTagPool.Has(refCellEntity))
                    visibleTagPool.Del(refCellEntity);

                if (!visibilityUpdateTagPool.Has(refCellEntity))
                    visibilityUpdateTagPool.Add(refCellEntity);
            }


            foreach (var refCoord in toShow)
            {
                var refIndex = CellIndexResolver(refCoord);

                if (!TryGetCellEntity(refIndex, out var refCellEntity, out var _))
                    continue;

                if (!visibilityRefPool.Has(refCellEntity))
                    continue;

                ref var visibilityRef = ref visibilityRefPool.Get(refCellEntity);
                visibilityRef.visibility.IncreaseVisibility();

                if (!visibleTagPool.Has(refCellEntity))
                    visibleTagPool.Add(refCellEntity);

                if (!exploredTagPool.Has(refCellEntity))
                    exploredTagPool.Add(refCellEntity); // will stay explored after hide

                if (!visibilityUpdateTagPool.Has(refCellEntity))
                    visibilityUpdateTagPool.Add(refCellEntity);
            }

            ListPool<HexCoordinates>.Add(toHide);
            ListPool<HexCoordinates>.Add(toShow);
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

