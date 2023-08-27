using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAutoProcessTurnSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<ProcessedTurnTag> processedTagPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag>,
            Exc<AwaitingVisualsTag, RunningVisualsTag, ProcessedTurnTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                processedTagPool.Value.Add(entity);
        }
    }
}
