using System;
using System.Security.Principal;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAftermathSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleComp> battlePool = default;
        private readonly EcsPoolInject<BattleAftermathComp> aftermathPool = default;
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;
        private readonly EcsPoolInject<DrainComp> drainPool = default;
        private readonly EcsPoolInject<GarbageTag> garbagePool = default;
        private readonly EcsPoolInject<RetireTag> retirePool = default;
        private readonly EcsPoolInject<BuffComp<NoStaminaDrainBuffTag>> staminaBuffPool = default;
        private readonly EcsPoolInject<DebuffTag<IntRangeValueComp<DamageRangeTag>>> debuffTagPool = default;

        private readonly EcsFilterInject<Inc<BattleComp, BattleAftermathComp>> aftermathFilter = default;
        private readonly EcsFilterInject<Inc<BuffComp<IntRangeValueComp<DamageRangeTag>>>> damageBuffFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

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
                            out _, out var playerEntity))
                        {
                            ref var opponentCellComp = ref cellPool.Value.Get(opponentEntity);
                            ref var playerCellComp = ref cellPool.Value.Get(playerEntity);

                            if (!drainPool.Value.Has(playerEntity))
                                drainPool.Value.Add(playerEntity);

                            ref var drainComp = ref drainPool.Value.Get(playerEntity);

                            if (!staminaBuffPool.Value.Has(playerEntity))
                            {
                                drainComp.Value += 10; // for cell transition
                            }
                            else
                            {
                                ref var staminaBuff = ref staminaBuffPool.Value.Get(playerEntity);
                                staminaBuff.Usages--;
                            }

                            drainComp.Value += 10; // for battle

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