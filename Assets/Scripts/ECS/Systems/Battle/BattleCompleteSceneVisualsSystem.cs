using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteSceneVisualsSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RunningVisualsTag> runPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, RunningVisualsTag>,
            Exc<GarbageTag>> filter1 = default;

        private readonly EcsFilterInject<
            Inc<RunningVisualsTag>,
            Exc<BattleTurnInfo>> filter2 = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var turnEntity in filter1.Value)
            {
                if (filter2.Value.GetEntitiesCount() == 0)
                {
                    runPool.Value.Del(turnEntity);
                }
            }
        }
    }
}
