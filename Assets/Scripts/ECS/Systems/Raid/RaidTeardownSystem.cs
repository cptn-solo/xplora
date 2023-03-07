using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidTeardownSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RetireTag> retirePool;
        private readonly EcsCustomInject<RaidService> raidService;

        private readonly EcsFilterInject<Inc<OpponentComp>> opponentFilter;

        public void Run(IEcsSystems systems)
        {
            if (raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out _))
                return;

            if (raidService.Value.PlayerEntity.Unpack(
                out _, out var playerEntity))
                retirePool.Value.Add(playerEntity);

            foreach (var opponentEntity in opponentFilter.Value)
                retirePool.Value.Add(opponentEntity);

        }
    }
}