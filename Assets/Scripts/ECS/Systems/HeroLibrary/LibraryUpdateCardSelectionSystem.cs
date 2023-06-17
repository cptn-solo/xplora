using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Library;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdateCardSelectionSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsPoolInject<SelectedTag> selectedPool = default;
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<RelationsMatrixComp> matrixPool = default;

        private readonly EcsFilterInject<
            Inc<RelationsMatrixComp>> matrixFilter = default;

        private readonly EcsFilterInject<
            Inc<HeroConfigRef,
                PositionComp,
                EntityViewRef<Hero>,
                UpdateTag<SelectedTag>>
            > toggleSelectionFilter = default; // entities to toggle selection on/off (depends on SelectedTag)

        private readonly EcsFilterInject<
            Inc<HeroConfigRef,
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
                    {
                        var sliderVisible = position2.Position.Team == playerTeamId &&
                            position1.Position.Team == playerTeamId;
                        card2.ToggleSliderVisibility(sliderVisible);
                        if (sliderVisible)
                            SetScoreSliderValue(card2, card1.PackedEntity.Value, card2.PackedEntity.Value);
                    }
                    else 
                        card2.ToggleSliderVisibility(false);
                }
            }
        }

        private void SetScoreSliderValue(HeroCard card, EcsPackedEntityWithWorld entity1, EcsPackedEntityWithWorld entity2)
        {
            foreach (var matrixEntity in matrixFilter.Value)
            {
                ref var matrixComp = ref matrixPool.Value.Get(matrixEntity);
                if (!matrixComp.Matrix.TryGetValue(new RelationsMatrixKey(entity1, entity2), out var scoreEntityPacked))
                    throw new Exception("No matrix value");

                if (!scoreEntityPacked.Unpack(out var world, out var scoreEntity))
                    throw new Exception("Stale score entity");

                var score = world.ReadIntValue<RelationScoreTag>(scoreEntity);
                card.SetSliderValue(score);                
            }
        }
    }
}

