using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDetectCompletedRoundSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<GarbageTag> garbageTagPool = default;

        private readonly EcsFilterInject<Inc<BattleRoundInfo>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var roundInfo = ref roundInfoPool.Value.Get(entity);

                if (roundInfo.QueuedHeroes.Length == 0)
                {
                    garbageTagPool.Value.Add(entity);

                    roundInfo.State = RoundState.RoundCompleted;
                    battleService.Value.NotifyRoundEventListeners(roundInfo);
                }
            }

        }
    }
}
