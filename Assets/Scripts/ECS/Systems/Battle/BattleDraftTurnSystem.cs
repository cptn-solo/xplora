using System;
using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDraftTurnSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> battleTurnPool = default;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        
        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                if (!battleService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                    throw new Exception("No battle");

                ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

                ref var turnInfo = ref battleTurnPool.Value.Get(entity);
                turnInfo.Turn = ++battleInfo.LastTurnNumber;
                
                turnInfo.State = TurnState.PrepareTurn;

                battleService.Value.TurnEntity = world.PackEntityWithWorld(entity);
                battleService.Value.NotifyTurnEventListeners(turnInfo);
            }
        }
    }
}
