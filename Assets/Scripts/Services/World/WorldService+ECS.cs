using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using System.Collections.Generic;
using Leopotam.EcsLite.ExtendedSystems;
using Random = UnityEngine.Random;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class WorldService  // ECS
    {
        internal EcsPackedEntityWithWorld WorldEntity { get; private set; }

        internal void SetWorldEntity(EcsPackedEntityWithWorld entity) =>
            WorldEntity = entity;

        private void StartEcsWorldContext()
        {
            ecsWorld = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsWorld);
            ecsInitSystems
                .Add(new WorldInitSystem())
                .Add(new TerrainInitSystem())
                .Add(new TerrainTypesInitSystem())
                .Add(new TerrainAttributesInitSystem())
                .Add(new WorldPoiInitSystem<PowerSourceComp>())
                .Add(new WorldPoiInitSystem<HPSourceComp>())
                .Add(new WorldPoiInitSystem<WatchTowerComp>())
                .Inject(this)
                .Init();

            ecsRunSystems = new EcsSystems(ecsWorld);
            ecsRunSystems
                .Add(new PresenceMonitorSystem())
                .Add(new TerrainGenerationSystem())
                .Add(new WorldProcessVisitorSystem<OpponentComp>())
                .Add(new WorldProcessPowerSourceVisitorSystem()) // checks for stamina
                .Add(new WorldProcessVisitorSystem<HPSourceComp>())
                .Add(new WorldProcessVisitorSystem<WatchTowerComp>())
                .Add(new WorldProcessVisitorSystem<TerrainAttributeComp>())
                .Add(new WorldVisitorSightUpdateSystem())
                .DelHere<VisitorComp>()
                .Add(new WorldVeilFieldCellsSystem())
                .DelHere<VeilCellsTag>()
                .Add(new WorldUnveilFieldCellsSystem())
                .DelHere<UnveilCellsTag>()
                .Add(new WorldVisibilityUpdateSystem())
                .Add(new WorldOutOfSightSystem())
                .Add(new WorldInSightSystem())
                .DelHere<VisibilityUpdateTag>()
                .Add(new DeployPoiSystem<PowerSourceComp>())
                .Add(new DeployPoiSystem<HPSourceComp>())
                .Add(new DeployPoiSystem<WatchTowerComp>())
                .DelHere<ProduceTag>()
                .Add(new UpdateWorldPoiSystem<PowerSourceComp>())
                .Add(new UpdateWorldPoiSystem<HPSourceComp>())
                .Add(new UpdateWorldPoiSystem<WatchTowerComp>())
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
            ecsRunSystems?.Destroy();
            ecsRunSystems = null;

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }

        private bool CheckEcsWorldIfReachable(int cellId)
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return false;

            var noGoPool = world.GetPool<NonPassableTag>();
            var worldPool = world.GetPool<WorldComp>();

            ref var worldComp = ref worldPool.Get(worldEntity);
            var packedCellEntity = worldComp.CellPackedEntities[cellId];

            if (!packedCellEntity.Unpack(world, out var cellEntity))
                return false;

            return !noGoPool.Has(cellEntity);
        }

        private void DestroyEcsWorldPoi()
        {
            if (!WorldEntity.Unpack(out var world, out _))
                return;

            var destroyTagPool = world.GetPool<DestroyTag>();
            var deployedPoiFilter = world
                .Filter<WorldPoiTag>()
                .Inc<EntityViewRef<bool>>()
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

        internal void VisitWorldCell(int prevCellIndex, int nextCellIndex, EcsPackedEntityWithWorld visitorEntity)
        {
            if (!TryGetCellEntity(nextCellIndex, out var cellEntity, out var world))
                return;

            var visitPool = world.GetPool<VisitorComp>();
            ref var visitComp = ref visitPool.Add(cellEntity);
            visitComp.Packed = visitorEntity;
            visitComp.PrevCellIndex = prevCellIndex;
            visitComp.NextCellIndex = nextCellIndex;
        }

        internal bool TryGetCellEntity(int cellIndex, out int entity, out EcsWorld world)
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

            var freeCellFilter = world
                .Filter<FieldCellComp>()
                .Exc<POIComp>()
                .Exc<NonPassableTag>()
                .Exc<TerrainAttributeComp>()
                .End();
            var cellPool = world.GetPool<FieldCellComp>();
            var cellCount = freeCellFilter.GetEntitiesCount();
            var sCount = count;
            var sLength = cellCount / sCount;

            List<int> segments = new();
            int[] freeIndexes = new int[sCount];

            for (int i = 0; i < sCount; i++)
                segments.Add(i);

            var en = freeCellFilter.AllEntities();

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

        internal void ResetEcsTerrain()
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            var explorationPool = world.GetPool<ExploredTag>();
            var terrainTypePool = world.GetPool<TerrainTypeComp>();

            var filter1 = world.Filter<FieldCellComp>().Inc<ExploredTag>().End();
            foreach (var entity in filter1)
                explorationPool.Del(entity);

            //TODO: revisit rettain reset for a new raid
            //var filter2 = world.Filter<FieldCellComp>().Inc<TerrainTypeComp>().End();
            //foreach (var entity in filter2)
            //    terrainTypePool.Del(entity);

        }

        internal void UnveilCellsInRange(int cellIndex, int range)
        {
            if (!WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            var pool = world.GetPool<UnveilCellsTag>();

            var coordinates = CellCoordinatesResolver(cellIndex);
            coordinates.RangeFromCoordinates(range, new Vector4(width, height), out var rangeCellIndexes);

            foreach (var idx in rangeCellIndexes)
            {
                if (!TryGetCellEntity(idx, out var entity, out _))
                    continue;

                if (!pool.Has(entity))
                    pool.Add(entity);
            }
        }
    }

}

