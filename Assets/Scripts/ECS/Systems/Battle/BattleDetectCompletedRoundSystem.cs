using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDetectCompletedRoundSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool;
        private readonly EcsPoolInject<GarbageTag> garbageTagPool;

        private readonly EcsFilterInject<Inc<BattleRoundInfo>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
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
