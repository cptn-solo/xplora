using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UnitComp> unitPool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;

        private readonly EcsFilterInject<Inc<DestroyTag, UnitComp>> destroyTagFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var unitComp = ref unitPool.Value.Get(entity);
                var destroyedUnit = unitComp.Unit;
                unitComp.Unit = null;

                unitPool.Value.Del(entity);
                destroyTagPool.Value.Del(entity);

                raidService.Value.UnitDestroyCallback(destroyedUnit);
            }
        }
    }
}