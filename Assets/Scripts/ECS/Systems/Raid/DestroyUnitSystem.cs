using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UnitRef> unitPool;
        private readonly EcsPoolInject<UnitOverlayRef> overlayPool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;

        private readonly EcsFilterInject<Inc<DestroyTag, UnitRef>> destroyTagFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var unitRef = ref unitPool.Value.Get(entity);
                var destroyedUnit = unitRef.Unit;
                unitRef.Unit = null;

                unitPool.Value.Del(entity);

                destroyTagPool.Value.Del(entity);

                raidService.Value.UnitDestroyCallback(destroyedUnit);
            }
        }
    }
}