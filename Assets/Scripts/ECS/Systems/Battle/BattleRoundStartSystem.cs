using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRoundStartSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<RoundInProgressTag> roundInProgressTagPool = default;

        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleInfoFilter = default;
        private readonly EcsFilterInject<Inc<BattleRoundInfo, RoundInProgressTag>> roundInfoFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in battleInfoFilter.Value)
            {
                if (roundInfoFilter.Value.GetEntitiesCount() == 0)
                {
                    ref var battleInfo = ref battleInfoPool.Value.Get(entity);
                    if (battleInfo.QueuedRounds.Length > 0 &&
                        battleInfo.QueuedRounds[0].Unpack(ecsWorld.Value, out var roundEntity))
                    {
                        roundInProgressTagPool.Value.Add(roundEntity);

                        ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);
                        roundInfo.State = RoundState.RoundInProgress;
                        battleService.Value.NotifyRoundEventListeners(roundInfo);
                    }
                }
            }
        }
    }
}
