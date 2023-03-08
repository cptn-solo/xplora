using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryDeployCardsSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<EntityViewFactoryRef<Hero>> factoryPool;
        private readonly EcsPoolInject<LibraryFieldComp> fieldPool;
        private readonly EcsPoolInject<PositionComp> positionPool;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool;

        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<Hero>>> factoryFilter;
        private readonly EcsFilterInject<Inc<LibraryFieldComp>> fieldFilter;
        private readonly EcsFilterInject<
            Inc<Hero, PositionComp>,
            Exc<EntityViewRef<Hero>>> filter;

        private readonly EcsCustomInject<HeroLibraryService> libraryService;

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
                        var card = factoryRef.FactoryRef(ecsWorld.Value.PackEntityWithWorld(entity));
                        card.DataLoader = libraryService.Value.GetDataForPackedEntity<Hero>;
                        slot.Put(card.Transform);
                        card.UpdateData();

                        ref var entityViewRef = ref entityViewRefPool.Value.Add(entity);
                        entityViewRef.EntityView = card;
                    }
                }
            }

            libraryService.Value.NotifyIfAllDataAvailable();
        }
    }
}

