using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDestroyDiedCardsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> cardViewRefPool = default;
        private readonly EcsPoolInject<EntityViewRef<BarsAndEffectsInfo>> overlayViewRefPool = default;
        private readonly EcsPoolInject<TransformRef<VisualsTransformTag>> visualsTransformRefPool = default;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>,
                EntityViewRef<BarsAndEffectsInfo>,
                ProcessedHeroTag,
                RetiredTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var overlayEntityViewRef = ref overlayViewRefPool.Value.Get(entity);
                overlayEntityViewRef.EntityView.Destroy();
                overlayViewRefPool.Value.Del(entity);

                ref var cardEntityViewRef = ref cardViewRefPool.Value.Get(entity);
                cardEntityViewRef.EntityView.Destroy();
                cardViewRefPool.Value.Del(entity);

                ref var visualsTransformRef = ref visualsTransformRefPool.Value.Get(entity);
                visualsTransformRefPool.Value.Del(entity);
            }
        }

    }
}
