using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class RetirePlayerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<DestroyTag> garbagePool;

        private readonly EcsFilterInject<Inc<PlayerComp, RetireTag>> playerToRetireFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in playerToRetireFilter.Value)
            {
                ref var playerComp = ref playerPool.Value.Get(entity);

                raidService.Value.RetireHero(playerComp.Hero);

                playerComp.Hero = default;
                playerComp.CellIndex = -1;

                garbagePool.Value.Add(entity);
            }
        }
    }
}