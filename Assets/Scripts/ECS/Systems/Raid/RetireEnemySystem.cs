using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RetireEnemySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<GarbageTag> garbagePool;

        private readonly EcsFilterInject<Inc<OpponentComp, HeroComp, RetireTag>> opponentToRetireFilter;
        
        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in opponentToRetireFilter.Value)
            {
                ref var heroComp = ref heroPool.Value.Get(entity);
                raidService.Value.RetireHero(heroComp.Hero);

                if (!garbagePool.Value.Has(entity))
                    garbagePool.Value.Add(entity);
            }
        }
    }
}