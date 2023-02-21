using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAutoProcessTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<CompletedTurnTag> completeTagPool;
        private readonly EcsPoolInject<ProcessedTurnTag> processedTagPool;        

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag>,
            Exc<ProcessedTurnTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleService.Value.PlayMode != BattleMode.Fastforward)
                return;

            foreach (var entity in filter.Value)
                processedTagPool.Value.Add(entity);
        }
    }
}
