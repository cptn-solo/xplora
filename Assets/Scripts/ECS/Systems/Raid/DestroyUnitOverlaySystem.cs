using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitOverlaySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UnitOverlayRef> overlayPool;

        private readonly EcsFilterInject<Inc<DestroyTag, UnitOverlayRef>> destroyTagFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {

                ref var overlayRef = ref overlayPool.Value.Get(entity);
                overlayRef.Overlay.ResetBars();
                overlayRef.Overlay.ResetOverlayInfo();

                overlayPool.Value.Del(entity);
            }

        }
    }
}