using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployPoiSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ProduceTag> produceTagPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<PoiRefComp> poiRefPool;

        private readonly EcsFilterInject<Inc<ProduceTag, POIComp, FieldCellComp>> produceTagFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in produceTagFilter.Value)
            {

                ref var cellComp = ref cellPool.Value.Get(entity);

                DeployWorldPoi callback = worldService.Value.POIDeploymentCallback;

                ref var poiRef = ref poiRefPool.Value.Add(entity);
                poiRef.PoiRef = callback(cellComp.CellIndex);

                produceTagPool.Value.Del(entity);
            }
        }
    }
}