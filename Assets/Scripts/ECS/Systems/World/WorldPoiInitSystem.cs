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

        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<WorldComp> worldPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<PowerSourceComp> psPool;
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<POIComp> poiPool;
        private readonly EcsPoolInject<WorldPoiTag> worldPoiTagPool;
        private readonly EcsPoolInject<GarbageTag> garbagePool;

        private readonly EcsFilterInject<Inc<FieldCellComp>, Exc<POIComp>> freeCellFilter;
        private readonly EcsFilterInject<Inc<PowerSourceComp>> powerSourceFilter;

        private readonly EcsCustomInject<WorldService> worldService;

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

                ref var powerComp = ref powerPool.Value.Add(freeCellEntity);
                powerComp.InitialValue = 10;

                // will prevent from spawning raid and other non static objects here
                worldPoiTagPool.Value.Add(freeCellEntity);

                // we don't need a separate entity for power source (may be yet)
                garbagePool.Value.Add(psEntity);
            }
        }

    }
}