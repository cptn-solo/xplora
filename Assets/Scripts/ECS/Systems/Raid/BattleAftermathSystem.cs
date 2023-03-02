﻿using System.Security.Principal;
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
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<DrainComp> drainPool;
        private readonly EcsPoolInject<VisitCellComp> visitPool;
        private readonly EcsPoolInject<GarbageTag> garbagePool;
        private readonly EcsPoolInject<RetireTag> retirePool;
        private readonly EcsPoolInject<BuffComp<NoStaminaDrainBuffTag>> staminaBuffPool;
        private readonly EcsPoolInject<DebuffTag<DamageRangeComp>> debuffTagPool;        

        private readonly EcsFilterInject<Inc<BattleComp, BattleAftermathComp>> aftermathFilter;
        private readonly EcsFilterInject<Inc<OpponentComp>> opponentFilter;
        private readonly EcsFilterInject<Inc<BuffComp<DamageRangeComp>>> damageBuffFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var battleEntity in aftermathFilter.Value)
            {
                ref var aftermathComp = ref aftermathPool.Value.Get(battleEntity);
                if (aftermathComp.Won)
                {
                    // clear battle buffs
                    foreach (var buffedEntity in damageBuffFilter.Value)
                        if (!debuffTagPool.Value.Has(buffedEntity))
                            debuffTagPool.Value.Add(buffedEntity);

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

                            if (!drainPool.Value.Has(playerEntity))
                                drainPool.Value.Add(playerEntity);

                            ref var drainComp = ref drainPool.Value.Get(playerEntity);

                            drainComp.Value += 10; // for cell transition

                            if (staminaBuffPool.Value.Has(playerEntity))
                                staminaBuffPool.Value.Del(playerEntity);
                            else
                                drainComp.Value += 10; // for battle (if no buff)

                            playerCellComp.CellIndex = opponentCellComp.CellIndex;
                        }

                        garbagePool.Value.Add(battleEntity);

                        raidService.Value.FinalizeWonBattle();
                    }
                }
                else
                {
                    // tag raid for teardown
                    raidService.Value.FinalizeRaid();
                }

            }            
        }
    }
}