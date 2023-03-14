using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidTerminationSystem : IEcsPostRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsCustomInject<RaidService> raidService = default;
        private readonly EcsCustomInject<WorldService> worldService = default;

        public void PostRun(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out _))
            {
                raidService.Value.StopEcsWorld();
                worldService.Value.ResetEcsTerrain();
            }
        }
    }
}