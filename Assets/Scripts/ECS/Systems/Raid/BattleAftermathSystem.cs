using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAftermathSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<BattleComp> battlePool;
        private readonly EcsPoolInject<BattleAftermathComp> aftermathPool;
        private readonly EcsPoolInject<OpponentComp> opponentPool;
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<RetireTag> retirePool;
        private readonly EcsPoolInject<DestroyTag> garbagePool;

        private readonly EcsFilterInject<Inc<BattleComp, BattleAftermathComp>> aftermathFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var battleEntity in aftermathFilter.Value)
            {
                ref var aftermathComp = ref aftermathPool.Value.Get(battleEntity);
                if (aftermathComp.Won)
                {
                    // tag opponent for delete from ecs and library
                    ref var battleComp = ref battlePool.Value.Get(battleEntity);
                    if (battleComp.EnemyPackedEntity.Unpack(
                        ecsWorld.Value, out var opponentEntity))
                    {
                        retirePool.Value.Add(opponentEntity);

                        // tag player for move to the opponent's cell
                        if (raidService.Value.PlayerEntity.Unpack(
                            ecsWorld.Value, out var playerEntity))
                        {
                            ref var opponentComp = ref opponentPool.Value.Get(opponentEntity);
                            ref var playerComp = ref playerPool.Value.Get(playerEntity);

                            playerComp.CellIndex = opponentComp.CellIndex;
                        }

                        raidService.Value.FinalizeWonBattle();
                    }
                }
                else
                {
                    // tag raid for teardown
                    if (raidService.Value.PlayerEntity.Unpack(
                        ecsWorld.Value, out var playerEntity))
                    {
                        retirePool.Value.Add(playerEntity);
                    }

                    raidService.Value.FinalizeLostBattle();
                }

                garbagePool.Value.Add(battleEntity);
            }            
        }
    }
}