using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleUpdateHoverUnitSystem : UpdateCardHoverSystem, IEcsRunSystem
    {
        private readonly EcsPoolInject<BarsInfoComp> barsInfoPool = default;

        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsPoolInject<EntityViewRef<HoverTag<Hero>>> detailsViewRefPool = default;
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> detailsBarsViewRefPool = default;

        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, EntityViewRef<Hero>,
                HoverTag>> hoverFilter = default;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<HoverTag<Hero>>,
                ItemsContainerRef<BarInfo>>> detailsViewFilter = default;


        public void Run(IEcsSystems systems)
        {
            foreach (var entity in hoverFilter.Value)
            {
                ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                var card = (ITransform)entityViewRef.EntityView;

                UpdateDetailsHover(entity, card.Transform);
            }

            if (hoverFilter.Value.GetEntitiesCount() == 0)
            {
                foreach (var detailsViewEntity in detailsViewFilter.Value)
                {
                    ref var detailsViewRef = ref detailsViewRefPool.Value.Get(detailsViewEntity);
                    var detailsView = (BattleUnitHover)detailsViewRef.EntityView;
                    detailsView.HeroName = null;
                }
            }

        }

        private void UpdateDetailsHover(int entity, Transform hostTransform)
        {
            foreach (var detailsViewEntity in detailsViewFilter.Value)
            {
                ref var barsInfo = ref barsInfoPool.Value.Get(entity);

                ref var detailsViewRef = ref detailsViewRefPool.Value.Get(detailsViewEntity);
                var detailsView = (BattleUnitHover)detailsViewRef.EntityView;
                detailsView.HeroName = barsInfo.Name;

                ref var detailsBarsRef = ref detailsBarsViewRefPool.Value.Get(detailsViewEntity);
                detailsBarsRef.Container.SetItems(barsInfo.BarsInfo);

                PositionHoverView(hostTransform, detailsView.Transform);
            }
        }
    }
}
