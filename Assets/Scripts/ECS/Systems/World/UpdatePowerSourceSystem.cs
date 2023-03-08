using Assets.Scripts.ECS.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateWorldPoiSystem<T> : IEcsRunSystem
        where T: struct
    {
        private readonly EcsPoolInject<EntityViewRef<bool>> poiRefPool;
        private readonly EcsPoolInject<UsedTag> usedTagPool;

        private readonly EcsFilterInject<
            Inc<T, UpdateTag, EntityViewRef<bool>>> updateFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);
                var entityView = (POI)poiRef.EntityView;
                entityView.Toggle(!usedTagPool.Value.Has(entity));
            }
        }
    }
}