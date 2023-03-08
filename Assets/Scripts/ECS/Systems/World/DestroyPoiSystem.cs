using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class DestroyPoiSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<bool>> poiRefPool;

        private readonly EcsFilterInject<Inc<DestroyTag, EntityViewRef<bool>>> destroyTagFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);
                poiRef.EntityView = null;

                poiRefPool.Value.Del(entity);
            }

        }
    }
}