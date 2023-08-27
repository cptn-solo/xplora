using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearResetRelationsEffectSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelEffectResetPendingTag> pool = default;

        private readonly EcsFilterInject<
            Inc<EffectInstanceInfo, RelEffectResetPendingTag>
            > filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                systems.GetWorld().ResetRelationEffect(entity, out var decrement);
                pool.Value.Del(entity);

                if (decrement != null)
                {
                    foreach (var item in decrement)
                        if (item.Unpack(out var origWorld, out var origEntity))
                            origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, origEntity);
                }
            }
        }
    }
}
