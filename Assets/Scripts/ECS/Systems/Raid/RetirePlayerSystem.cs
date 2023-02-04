using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class RetirePlayerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<GarbageTag> garbagePool;

        private readonly EcsFilterInject<Inc<PlayerComp, HeroComp, RetireTag>> playerToRetireFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in playerToRetireFilter.Value)
            {
                ref var heroComp = ref heroPool.Value.Get(entity);

                raidService.Value.RetireHero(heroComp.Hero);

                garbagePool.Value.Add(entity);
            }
        }
    }
}