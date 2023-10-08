using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidUIInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<ResetTag<CollapseKindsTag>> resetKindsPanelPool = default;
        public void Init(IEcsSystems systems)
        {
            // kinds panel is initially collapsed
            resetKindsPanelPool.Value.Add(systems.GetWorld().NewEntity());
        }
    }
}