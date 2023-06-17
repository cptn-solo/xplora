using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamCardsSpawnSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<EntityViewRef<TeamMemberInfo>> pool = default;
        private readonly EcsPoolInject<TransformRef<Team>> containerPool = default;
        private readonly EcsPoolInject<UpdateTag<HpTag>> updateHPTagPool = default;

        private readonly EcsPoolInject<EntityViewFactoryRef<TeamMemberInfo>> factoryPool = default;
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<TeamMemberInfo>>> factoryFilter = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, HeroConfigRef>,
            Exc<EntityViewRef<TeamMemberInfo>>> filter = default;

        private readonly EcsFilterInject<
            Inc<TransformRef<Team>>> containerFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

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