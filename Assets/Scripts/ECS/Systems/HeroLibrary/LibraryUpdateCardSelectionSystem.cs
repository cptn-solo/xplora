﻿using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Library;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdateCardSelectionSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<Hero> heroConfigPool;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool;
        private readonly EcsPoolInject<EntityViewRef<SelectedTag<Hero>>> detailsViewRefPool;
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> detailsBarsViewRefPool;
        
        private readonly EcsPoolInject<SelectedTag> selectedTagPool;
        private readonly EcsPoolInject<UpdateTag<SelectedTag>> updateSelectionTagPool;

        private readonly EcsFilterInject<
            Inc<Hero, EntityViewRef<Hero>, SelectedTag>,
            Exc<UpdateTag<SelectedTag>>> oldSelectionFilter;

        private readonly EcsFilterInject<
            Inc<Hero, EntityViewRef<Hero>, UpdateTag<SelectedTag>>,
            Exc<SelectedTag>> selectionFilter;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<SelectedTag<Hero>>, ItemsContainerRef<BarInfo>>> detailsViewFilter;


        public void Run(IEcsSystems systems)
        {
            foreach (var entity in selectionFilter.Value)
            {
                ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                var card = (HeroCard)entityViewRef.EntityView;
                card.Selected = true;

                RemoveCurrentSelection();

                selectedTagPool.Value.Add(entity);

                UpdateDetailsHover(entity);

                updateSelectionTagPool.Value.Del(entity);
            }
        }

        private void UpdateDetailsHover(int entity)
        {
            var hero = heroConfigPool.Value.Get(entity);

            foreach (var detailsViewEntity in detailsViewFilter.Value)
            {
                ref var detailsViewRef = ref detailsViewRefPool.Value.Get(detailsViewEntity);
                var detailsView = (HeroDetailsHover)detailsViewRef.EntityView;
                detailsView.Hero = hero;

                ref var detailsBarsRef = ref detailsBarsViewRefPool.Value.Get(detailsViewEntity);
                detailsBarsRef.Container.SetItems(hero.BarsInfo);
            }
        }

        private void RemoveCurrentSelection()
        {
            foreach (var entity in oldSelectionFilter.Value)
            {
                ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                var card = (HeroCard)entityViewRef.EntityView;
                card.Selected = false;

                selectedTagPool.Value.Del(entity);
            }

        }
    }
}

