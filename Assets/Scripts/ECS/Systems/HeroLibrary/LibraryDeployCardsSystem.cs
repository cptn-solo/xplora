using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI.Library;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryDeployCardsSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<EntityViewFactoryRef<Hero>> factoryPool = default;
        private readonly EcsPoolInject<LibraryFieldComp> fieldPool = default;
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsPoolInject<UpdateTag<RelationsMatrixComp>> updateRelContextTagPool = default;
        

        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<Hero>>> factoryFilter = default;
        private readonly EcsFilterInject<Inc<LibraryFieldComp>> fieldFilter = default;
        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, PositionComp>,
            Exc<EntityViewRef<Hero>>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public void Run(IEcsSystems systems)
        {
            if (filter.Value.GetEntitiesCount() == 0)
                return;

            if (fieldFilter.Value.GetEntitiesCount() == 0)
                return;

            if (factoryFilter.Value.GetEntitiesCount() == 0)
                return;

            foreach (var fieldEntity in fieldFilter.Value)
            {
                foreach (var factoryEntity in factoryFilter.Value)
                {
                    ref var factoryRef = ref factoryPool.Value.Get(factoryEntity);
                    ref var field = ref fieldPool.Value.Get(fieldEntity);

                    foreach (var entity in filter.Value)
                    {
                        ref var pos = ref positionPool.Value.Get(entity);
                        var slot = field.Slots[pos.Position];
                        var card = (HeroCard)factoryRef.FactoryRef(ecsWorld.Value.PackEntityWithWorld(entity));
                        card.DataLoader = libraryService.Value.GetHeroConfigForLibraryHeroInstance;
                        slot.Put(card.Transform);
                        card.UpdateData();

                        ref var entityViewRef = ref entityViewRefPool.Value.Add(entity);
                        entityViewRef.EntityView = card;

                        card.ToggleSliderVisibility(false);
                    }
                }
            }

            if (libraryService.Value.PlayerTeamEntity.Unpack(out _, out var playerTeamEntity) &&
                !updateRelContextTagPool.Value.Has(playerTeamEntity))
                updateRelContextTagPool.Value.Add(playerTeamEntity);

            libraryService.Value.NotifyIfAllDataAvailable();
        }
    }
}

