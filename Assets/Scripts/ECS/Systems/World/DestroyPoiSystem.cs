using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class DestroyPoiSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EntityViewRef<bool>> poiRefPool = default;

        private readonly EcsFilterInject<Inc<DestroyTag, EntityViewRef<bool>>> destroyTagFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);
                poiRef.EntityView.Destroy();
                poiRef.EntityView = null;

                poiRefPool.Value.Del(entity);
            }

        }
    }
}