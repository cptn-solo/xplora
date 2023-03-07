using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;
using Assets.Scripts.World;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployPoiSystem<T> : IEcsRunSystem
        where T: struct
    {
        private readonly EcsPoolInject<UpdateTag> updateTagPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<PoiRef> poiRefPool;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool;

        private readonly EcsFilterInject<
            Inc<T, ProduceTag, WorldPoiTag, FieldCellComp>,
            Exc<PoiRef>> produceTagFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in produceTagFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);

                DeployWorldPoi callback = worldService.Value.POIDeploymentCallback;

                ref var poiRef = ref poiRefPool.Value.Add(entity);
                poiRef.Poi = callback(cellComp.CellIndex);
                poiRef.Poi.SetupAnimator<T>();
                poiRef.Poi.Toggle(true);

                updateTagPool.Value.Add(entity);
            }
        }
    }
}