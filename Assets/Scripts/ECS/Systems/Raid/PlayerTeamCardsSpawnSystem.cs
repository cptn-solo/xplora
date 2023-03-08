using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamCardsSpawnSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<EntityViewRef<TeamMemberInfo>> pool;
        private readonly EcsPoolInject<TransformRef<Team>> containerPool;
        private readonly EcsPoolInject<UpdateHPTag> updateHPTagPool;

        private readonly EcsPoolInject<EntityViewFactoryRef<TeamMemberInfo>> factoryPool;
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<TeamMemberInfo>>> factoryFilter;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, HeroConfigRefComp>,
            Exc<EntityViewRef<TeamMemberInfo>>> filter;

        private readonly EcsFilterInject<
            Inc<TransformRef<Team>>> containerFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            if (containerFilter.Value.GetEntitiesCount() == 0)
                return;

            foreach (var factoryEntity in factoryFilter.Value)
            {
                ref var factoryRef = ref factoryPool.Value.Get(factoryEntity);

                foreach (var containerEntity in containerFilter.Value)
                {
                    ref var containerRef = ref containerPool.Value.Get(containerEntity);

                    foreach (var entity in filter.Value)
                    {
                        ref var entityViewRef = ref pool.Value.Add(entity);
                        var card =
                            factoryRef
                                .FactoryRef(ecsWorld.Value.PackEntityWithWorld(entity));
                        card.DataLoader = raidService.Value.GetTeamMemberInfoForPackedEntity;
                        card.Transform.SetParent(containerRef.Transform);
                        card.UpdateData();

                        entityViewRef.EntityView = card;

                        updateHPTagPool.Value.Add(entity);
                    }
                }
            }

        }
    }
}