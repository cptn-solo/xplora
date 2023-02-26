using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDestroyDiedCardsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> cardViewRefPool;
        private readonly EcsPoolInject<EntityViewRef<BarsAndEffectsInfo>> overlayViewRefPool;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>,
                EntityViewRef<BarsAndEffectsInfo>,
                RetiredTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleService.Value.PlayMode != BattleMode.Fastforward)
                return;

            foreach (var entity in filter.Value)
            {
                ref var overlayEntityViewRef = ref overlayViewRefPool.Value.Get(entity);
                overlayEntityViewRef.EntityView.Destroy();
                overlayViewRefPool.Value.Del(entity);

                ref var cardEntityViewRef = ref cardViewRefPool.Value.Get(entity);
                cardEntityViewRef.EntityView.Destroy();
                cardViewRefPool.Value.Del(entity);
            }
        }

    }
}
