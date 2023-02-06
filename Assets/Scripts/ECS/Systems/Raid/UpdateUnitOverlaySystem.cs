using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateUnitOverlaySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<UnitOverlayRef> overlayPool;

        private readonly EcsFilterInject<
            Inc<UpdateTag, UnitOverlayRef, PowerComp>> updateFilter;


        public void Run(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var powerComp = ref powerPool.Value.Get(entity);
                ref var overlayRef = ref overlayPool.Value.Get(entity);

                overlayRef.Overlay.SetBarsInfo(powerComp.BarsInfo);
            }

        }
    }
}