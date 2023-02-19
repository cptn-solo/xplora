using System;
using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDraftTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> battleTurnPool;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<BattleTurnRefComp> turnRefPool;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                if (!battleService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                    throw new Exception("No battle");

                ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

                ref var turnInfo = ref battleTurnPool.Value.Get(entity);

                BattleTurnInfo.Update(ref turnInfo, ++battleInfo.LastTurnNumber, Hero.Default, Hero.Default);

                turnInfo.State = TurnState.PrepareTurn;

                battleService.Value.NotifyTurnEventListeners(turnInfo);
            }
        }
    }
}
