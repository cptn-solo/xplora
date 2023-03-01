using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamCardsUnlinkSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<TeamMemberInfo>> pool;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<TeamMemberInfo>>> filter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            if (raidService.Value.TeamMemberFactory != null)
                return;

            foreach (var entity in filter.Value)
            {
                ref var entityViewRef = ref pool.Value.Get(entity);
                entityViewRef.EntityView = null;
                pool.Value.Del(entity);
            }
        }
    }
}