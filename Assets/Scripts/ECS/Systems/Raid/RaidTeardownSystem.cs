using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidTeardownSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<RetireTag> retirePool = default;
        private readonly EcsCustomInject<RaidService> raidService = default;

        private readonly EcsFilterInject<Inc<OpponentComp>> opponentFilter = default;

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