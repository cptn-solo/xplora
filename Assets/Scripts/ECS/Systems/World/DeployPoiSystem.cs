using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;
using Assets.Scripts.World;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployPoiSystem<T> : IEcsRunSystem
        where T: struct
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<UpdateTag> updateTagPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<EntityViewRef<bool>> poiRefPool;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool;

        private readonly EcsPoolInject<EntityViewFactoryRef<bool>> factoryRefPool;
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<bool>>> factoryRefFilter;

        private readonly EcsFilterInject<
            Inc<T, ProduceTag, WorldPoiTag, FieldCellComp>,
            Exc<EntityViewRef<bool>>> produceTagFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var factoryEntity in factoryRefFilter.Value)
            {
                ref var factoryRef = ref factoryRefPool.Value.Get(factoryEntity);

                foreach (var entity in produceTagFilter.Value)
                {
                    ref var cellComp = ref cellPool.Value.Get(entity);

                    var entityView = (POI)factoryRef.FactoryRef(ecsWorld.Value.PackEntityWithWorld(entity));

                    var coord = worldService.Value.CellCoordinatesResolver(cellComp.CellIndex);
                    var pos = worldService.Value.WorldPositionResolver(coord);

                    entityView.Transform.SetPositionAndRotation(pos, Quaternion.identity);
                    entityView.SetupAnimator<T>();
                    entityView.Toggle(true);

                    ref var poiRef = ref poiRefPool.Value.Add(entity);
                    poiRef.EntityView = entityView;

                    updateTagPool.Value.Add(entity);
                }
            }

        }
    }
}