using System.Collections.Generic;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldPoiInitSystem<T> : IEcsInitSystem
        where T: struct
    {
        private readonly EcsPoolInject<WorldComp> worldPool = default;
        private readonly EcsPoolInject<T> psPool = default;
        private readonly EcsPoolInject<POIComp> poiPool = default;
        private readonly EcsPoolInject<WorldPoiTag> worldPoiTagPool = default;
        private readonly EcsPoolInject<TerrainAttributeComp> terrainAttributePool = default;

        private readonly EcsFilterInject<Inc<FieldCellComp>, Exc<POIComp>> freeCellFilter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Init(IEcsSystems systems)
        {
            if (!worldService.Value.WorldEntity.Unpack(out var world, out var ent))
                return;

            ref var worldComp = ref worldPool.Value.Get(ent);

            var cellCount = freeCellFilter.Value.GetEntitiesCount();
            var sCount = worldComp.POICountForType<T>();

            // ignore terrain attributes already placed:
            var freeIndexes = worldService.Value.GetRandomFreeCellIndexes(sCount, true);

            int i = -1;
            for (var idx = 0; idx < sCount; idx++)
            {
                var freeCellIndex = freeIndexes[++i];

                if (!worldComp.CellPackedEntities[freeCellIndex].Unpack(world, out var freeCellEntity))
                    continue;

                ref var poiComp = ref poiPool.Value.Add(freeCellEntity);
                ref var psComp = ref psPool.Value.Add(freeCellEntity);

                // override terrain attributes if any:
                if (terrainAttributePool.Value.Has(freeCellEntity))
                    terrainAttributePool.Value.Del(freeCellEntity);

                // will prevent from spawning raid and other non static objects here
                worldPoiTagPool.Value.Add(freeCellEntity);

            }
        }

    }
}