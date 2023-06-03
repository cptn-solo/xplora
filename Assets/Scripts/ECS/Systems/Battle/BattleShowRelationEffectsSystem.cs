using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleShowRelationEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<RelationEffectInfo>> pool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;

        private readonly EcsFilterInject<
            Inc<ItemsContainerRef<RelationEffectInfo>,
                RelationEffectsComp,
                UpdateTag<RelationEffectInfo>>
            > filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var relationEffects = ref relEffectsPool.Value.Get(entity);
                var buffer = ListPool<RelationEffectInfo>.Get();

                foreach (var item in relationEffects.CurrentEffects)
                    buffer.Add(item.Value.EffectInfo);
                
                ref var viewRef = ref pool.Value.Get(entity);
                viewRef.Container.Reset();
                viewRef.Container.SetInfo(buffer.ToArray());

                ListPool<RelationEffectInfo>.Add(buffer);
            }
        }
    }
}
