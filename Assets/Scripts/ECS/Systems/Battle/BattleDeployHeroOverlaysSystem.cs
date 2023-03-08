using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI.Battle;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDeployHeroOverlaysSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewFactoryRef<BarsAndEffectsInfo>> factoryPool;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool;
        private readonly EcsPoolInject<EntityViewRef<BarsAndEffectsInfo>> overlayRefPool;

        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<BarsAndEffectsInfo>>> factoryFilter;
        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>>,
            Exc<EntityViewRef<BarsAndEffectsInfo>>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var factoryEntity in factoryFilter.Value)
            {
                ref var factoryRef = ref factoryPool.Value.Get(factoryEntity);

                foreach (var entity in filter.Value)
                {
                    ref var entityView = ref entityViewRefPool.Value.Get(entity);
                    var battleUnit = (BattleUnit)entityView.EntityView;
                    var overlay = (Overlay)factoryRef.FactoryRef(entityView.EntityView.PackedEntity);
                    battleUnit.HeroAnimation.SetOverlay(overlay);

                    overlay.DataLoader = battleService.Value.GetDataForPackedEntity<BarsAndEffectsInfo>;
                    overlay.UpdateData();

                    ref var overlayRef = ref overlayRefPool.Value.Add(entity);
                    overlayRef.EntityView = overlay;
                }
            }
        }
    }
}
