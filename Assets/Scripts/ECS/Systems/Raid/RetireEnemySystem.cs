using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class RetireEnemySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<OpponentComp> opponentPool;
        private readonly EcsPoolInject<DestroyTag> garbagePool;

        private readonly EcsFilterInject<Inc<OpponentComp, RetireTag>> opponentToRetireFilter;
        
        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in opponentToRetireFilter.Value)
            {
                ref var opponentComp = ref opponentPool.Value.Get(entity);
                raidService.Value.RetireHero(opponentComp.Hero);
                opponentComp.Hero = default;
                opponentComp.CellIndex = -1;

                garbagePool.Value.Add(entity);
            }
        }
    }
}