using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearUsedRelationsEffectSystem : BaseEcsSystem
    {
        private readonly EcsFilterInject<
            Inc<PositionComp>
            > filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                systems.GetWorld().RemoveExpiredRelEffects(entity, out var decrement);

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
