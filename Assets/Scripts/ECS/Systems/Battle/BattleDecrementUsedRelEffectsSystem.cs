using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public partial class BattleDecrementUsedRelEffectsSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<DecrementPendingTag> decrementPool = default;
        private readonly EcsFilterInject<Inc<DecrementPendingTag, EffectInstanceInfo>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var entity in filter.Value)
            {
                world.UseRelationEffect(entity);
                decrementPool.Value.Del(entity);
            }
        }
    }

}
