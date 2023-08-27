using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI.Battle;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDeployHeroOverlaysSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EntityViewFactoryRef<BarsAndEffectsInfo>> factoryPool = default;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsPoolInject<EntityViewRef<BarsAndEffectsInfo>> overlayRefPool = default;

        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<BarsAndEffectsInfo>>> factoryFilter = default;
        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>>,
            Exc<EntityViewRef<BarsAndEffectsInfo>>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
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
