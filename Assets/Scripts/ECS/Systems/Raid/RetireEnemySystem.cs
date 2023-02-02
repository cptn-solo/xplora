using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class RetireEnemySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<DestroyTag> garbagePool;

        private readonly EcsFilterInject<Inc<OpponentComp, RetireTag>> opponentToRetireFilter;
        
        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in opponentToRetireFilter.Value)
            {
                ref var heroComp = ref heroPool.Value.Get(entity);
                raidService.Value.RetireHero(heroComp.Hero);

                garbagePool.Value.Add(entity);
            }
        }
    }
}