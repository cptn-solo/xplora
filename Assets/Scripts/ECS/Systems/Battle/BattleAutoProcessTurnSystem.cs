using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAutoProcessTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ProcessedTurnTag> processedTagPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag>,
            Exc<ProcessedTurnTag>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            if (battleService.Value.PlayMode != BattleMode.Fastforward)
                return;

            foreach (var entity in filter.Value)
                processedTagPool.Value.Add(entity);
        }
    }
}
