using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitOverlaySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<UnitInfo>> overlayPool = default;

        private readonly EcsFilterInject<Inc<DestroyTag, EntityViewRef<UnitInfo>>> destroyTagFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {

                ref var overlayRef = ref overlayPool.Value.Get(entity);
                overlayRef.EntityView.Destroy();
                overlayRef.EntityView = null;

                overlayPool.Value.Del(entity);
            }

        }
    }
}