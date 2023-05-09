using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Library;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdateCardSelectionSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsPoolInject<SelectedTag> selectedPool = default;
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<Team> teamPool = default;

        private readonly EcsFilterInject<
            Inc<Hero,
                PositionComp,
                EntityViewRef<Hero>,
                UpdateTag<SelectedTag>>
            > toggleSelectionFilter = default; // entities to toggle selection on/off (depends on SelectedTag)

        private readonly EcsFilterInject<
            Inc<Hero,
                PositionComp,
                EntityViewRef<Hero>>,
            Exc<UpdateTag<SelectedTag>>
            > removeSelectionFilter = default; // the rest - to just clear selection visuals

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var toggleEntity in toggleSelectionFilter.Value)
            {
                ref var toggleSelectionView = ref entityViewRefPool.Value.Get(toggleEntity);
                ref var position1 = ref positionPool.Value.Get(toggleEntity);
                HeroCard card1 = (HeroCard)toggleSelectionView.EntityView;
                var selected1 = selectedPool.Value.Has(toggleEntity);
                card1.Selected = selected1;

                var playerTeamId = libraryService.Value.PlayerTeam.Id;
                // sliders shown only on player cards added to a team and _not_ yet selected (to show _relative_ score with
                // the selected card)
                card1.ToggleSliderVisibility(false);
                
                foreach (var deselectEntity in removeSelectionFilter.Value)
                {
                    if (selectedPool.Value.Has(deselectEntity))
                        selectedPool.Value.Del(deselectEntity);

                    ref var removeSelectionView = ref entityViewRefPool.Value.Get(deselectEntity);
                    var card2 = (HeroCard)removeSelectionView.EntityView;
                    card2.Selected = false;

                    ref var position2 = ref positionPool.Value.Get(deselectEntity);

                    if (selected1) // toggle sliders only if there is a selected card 
                        card2.ToggleSliderVisibility(position2.Position.Team == playerTeamId);
                    else 
                        card2.ToggleSliderVisibility(false);
                }
            }
        }
    }
}

