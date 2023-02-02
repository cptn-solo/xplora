﻿using Assets.Scripts.ECS.Data;
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
        private readonly EcsPoolInject<FieldCellComp> cellPool;
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
                            ref var opponentCellComp = ref cellPool.Value.Get(opponentEntity);
                            ref var playerCellComp = ref cellPool.Value.Get(playerEntity);

                            playerCellComp.CellIndex = opponentCellComp.CellIndex;
                        }

                        ecsWorld.Value.DelEntity(battleEntity);

                        raidService.Value.FinalizeWonBattle();
                    }
                }
                else
                {
                    // tag raid for teardown
                    if (raidService.Value.PlayerEntity.Unpack(
                        ecsWorld.Value, out var playerEntity))
                        retirePool.Value.Add(playerEntity);

                    if (raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                        ecsWorld.Value.DelEntity(raidEntity);
                }

            }            
        }
    }
}