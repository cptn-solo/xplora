using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearProcessedHeroEffectsSystem<T> : BaseEcsSystem where T : struct
    {
        private readonly EcsFilterInject<Inc<ProcessedHeroTag, T>> filter = default;
        private readonly EcsPoolInject<T> pool = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                pool.Value.Del(entity);
        }
    }
}
