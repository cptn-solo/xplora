using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Library;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdateCardSelectionSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsPoolInject<SelectedTag> selectedPool = default;

        private readonly EcsFilterInject<
            Inc<Hero,
                EntityViewRef<Hero>,
                UpdateTag<SelectedTag>>
            > setSelectionFilter = default;

        private readonly EcsFilterInject<
            Inc<Hero,
                EntityViewRef<Hero>>,
            Exc<SelectedTag>
            > removeSelectionFilter = default;


        public void Run(IEcsSystems systems)
        {
            foreach (var toggleEntity in setSelectionFilter.Value)
            {
                ref var toggleSelectionView = ref entityViewRefPool.Value.Get(toggleEntity);
                HeroCard card = (HeroCard)toggleSelectionView.EntityView;
                card.Selected = selectedPool.Value.Has(toggleEntity);

                foreach (var deselectEntity in removeSelectionFilter.Value)
                {
                    ref var removeSelectionView = ref entityViewRefPool.Value.Get(deselectEntity);
                    card = (HeroCard)removeSelectionView.EntityView;
                    card.Selected = false;
                }
            }
        }
    }
}

