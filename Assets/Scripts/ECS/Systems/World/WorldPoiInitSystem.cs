using System.Collections.Generic;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldPoiInitSystem : IEcsInitSystem
    {

        private EcsWorldInject ecsWorld;

        private EcsPoolInject<WorldComp> worldPool;
        private EcsPoolInject<FieldCellComp> cellPool;
        private EcsPoolInject<PowerSourceComp> psPool;
        private EcsPoolInject<POIComp> poiPool;
        private EcsPoolInject<WorldPoiTag> worldPoiTagPool;
        private EcsPoolInject<GarbageTag> garbagePool;

        private EcsFilterInject<Inc<FieldCellComp>, Exc<POIComp>> freeCellFilter;
        private EcsFilterInject<Inc<PowerSourceComp>> powerSourceFilter;

        private EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            if (!worldService.Value.WorldEntity.Unpack(out var world, out var ent))
                return;

            ref var worldComp = ref worldPool.Value.Get(ent);

            var cellCount = freeCellFilter.Value.GetEntitiesCount();
            var sCount = powerSourceFilter.Value.GetEntitiesCount();
            var freeIndexes = worldService.Value.GetRandomFreeCellIndexes(sCount);

            int i = -1;
            foreach (var psEntity in powerSourceFilter.Value)
            {
                var freeCellIndex = freeIndexes[++i];

                if (!worldComp.CellPackedEntities[freeCellIndex].Unpack(world, out var freeCellEntity))
                    continue;

                ref var poiComp = ref poiPool.Value.Add(freeCellEntity);
                ref var psComp = ref psPool.Value.Add(freeCellEntity);

                // will prevent from spawning raid and other non static objects here
                worldPoiTagPool.Value.Add(freeCellEntity);

                // we don't need a separate entity for power source (may be yet)
                garbagePool.Value.Add(psEntity);
            }
        }

    }
}