using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRunSceneVisualsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<SceneVisualsQueueComp> visualsQueueComp = default;
        private readonly EcsPoolInject<RunningVisualsTag> runPool = default;
        private readonly EcsPoolInject<AwaitingVisualsTag> waitPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, AwaitingVisualsTag, SceneVisualsQueueComp>,
            Exc<RunningVisualsTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var turnEntity in filter.Value) 
            {
                ref var queueComp = ref visualsQueueComp.Value.Get(turnEntity);
                EcsPackedEntity? item = queueComp.QueuedVisuals.Count > 0 ? queueComp.QueuedVisuals[0] : null;

                if (item != null && item.Value.Unpack(systems.GetWorld(), out var visualEntity))
                {
                    // TODO: 
                    // 1. get appropriate view ref
                    // 2. run animation/action + pass callback to remove RunningVisualsTag from visualEntity when finished

                    runPool.Value.Add(turnEntity);
                    runPool.Value.Add(visualEntity);
                    
                    queueComp.QueuedVisuals.RemoveAt(0);
                }
                else
                {
                    // empty queue
                    waitPool.Value.Del(turnEntity);
                }

            }
        }
    }
}
