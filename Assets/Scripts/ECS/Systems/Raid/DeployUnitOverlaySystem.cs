using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployUnitOverlaySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UnitRef> unitPool;
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<UnitOverlayRef> overlayPool;

        private readonly EcsFilterInject<
            Inc<UnitRef, PlayerComp, PowerComp>,
            Exc<UnitOverlayRef>> produceTagFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in produceTagFilter.Value)
            {
                ref var unitRef = ref unitPool.Value.Get(entity);
                ref var powerComp = ref powerPool.Value.Get(entity);

                ref var overlayRef = ref overlayPool.Value.Add(entity);
                overlayRef.Overlay = raidService.Value.UnitOverlaySpawner(
                    unitRef.Unit.transform);
                overlayRef.Overlay.ResetOverlayInfo();
                overlayRef.Overlay.SetBarsInfo(powerComp.BarsInfo);                
            }

        }
    }
}