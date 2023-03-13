using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI.Battle;
using UnityEngine;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleUpdateHoverUnitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;

        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool;
        private readonly EcsPoolInject<EntityViewRef<SelectedTag<Hero>>> detailsViewRefPool;
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> detailsBarsViewRefPool;
        private readonly EcsPoolInject<SelectedTag> selectedTagPool;
        private readonly EcsPoolInject<DeselectTag> deselectTagPool;

        private readonly EcsPoolInject<UpdateTag<SelectedTag>> updateSelectionTagPool;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>, SelectedTag>,
            Exc<UpdateTag<SelectedTag>>> oldSelectionFilter;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>, DeselectTag>> deselectionFilter;

        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, EntityViewRef<Hero>, UpdateTag<SelectedTag>>,
            Exc<SelectedTag>> selectionFilter;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<SelectedTag<Hero>>, ItemsContainerRef<BarInfo>>> detailsViewFilter;


        public void Run(IEcsSystems systems)
        {
            foreach (var entity in deselectionFilter.Value)
            {
                foreach (var detailsViewEntity in detailsViewFilter.Value)
                {
                    ref var detailsViewRef = ref detailsViewRefPool.Value.Get(detailsViewEntity);
                    var detailsView = (BattleUnitHover)detailsViewRef.EntityView;
                    detailsView.HeroName = null;
                }

                deselectTagPool.Value.Del(entity);

                if (selectedTagPool.Value.Has(entity))
                    selectedTagPool.Value.Del(entity);
            }

            foreach (var entity in selectionFilter.Value)
            {
                ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                var card = (BattleUnit)entityViewRef.EntityView;

                RemoveCurrentSelection();

                selectedTagPool.Value.Add(entity);

                UpdateDetailsHover(entity, card.Transform);

                updateSelectionTagPool.Value.Del(entity);
            }
        }

        private void UpdateDetailsHover(int entity, Transform hostTransform)
        {
            foreach (var detailsViewEntity in detailsViewFilter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(entity);
                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(libEntity);

                ref var detailsViewRef = ref detailsViewRefPool.Value.Get(detailsViewEntity);
                var detailsView = (BattleUnitHover)detailsViewRef.EntityView;
                detailsView.HeroName = heroConfig.Name;

                ref var detailsBarsRef = ref detailsBarsViewRefPool.Value.Get(detailsViewEntity);
                detailsBarsRef.Container.SetItems(heroConfig.BarsInfo);

                // TODO: sync hover position with card requested hover
                detailsView.Transform.position = hostTransform.position;

            }
        }

        private void RemoveCurrentSelection()
        {
            foreach (var entity in oldSelectionFilter.Value)
            {
                ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                var card = (BattleUnit)entityViewRef.EntityView;

                selectedTagPool.Value.Del(entity);
            }

        }
    }
}
