using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidTerminationSystem : IEcsPostRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsCustomInject<RaidService> raidService;
        private readonly EcsCustomInject<WorldService> worldService;

        public void PostRun(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out _))
            {
                raidService.Value.StopEcsRaidContext();
                worldService.Value.ResetEcsTerrain();
            }
        }
    }
}