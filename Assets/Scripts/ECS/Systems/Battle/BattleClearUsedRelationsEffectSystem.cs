using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearUsedRelationsEffectSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;

        private readonly EcsFilterInject<
            Inc<RelationEffectsComp, ProcessedHeroTag>
            > filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var relEffects = ref relEffectsPool.Value.Get(entity);
                relEffects.RemoveExpired(out var decrement);

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
