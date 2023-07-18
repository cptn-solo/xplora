using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleSceneVisualsCompleteSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ScheduleVisualsTag> schedulePool = default;
        private readonly EcsPoolInject<AwaitingVisualsTag> waitPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag>,
            Exc<AwaitingVisualsTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var turnEntity in filter.Value)
            {
                // prevent reuse:
                schedulePool.Value.Del(turnEntity);
                waitPool.Value.Add(turnEntity);
            }                
        }
    }
}
