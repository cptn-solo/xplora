using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitOverlaySystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EntityViewRef<UnitInfo>> overlayPool = default;

        private readonly EcsFilterInject<Inc<DestroyTag, EntityViewRef<UnitInfo>>> destroyTagFilter = default;

        public override void RunIfActive(IEcsSystems systems)
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