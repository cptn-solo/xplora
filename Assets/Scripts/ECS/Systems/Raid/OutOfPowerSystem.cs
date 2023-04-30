using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class OutOfPowerSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<OutOfPowerTag, PlayerComp>> oopFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            if (oopFilter.Value.GetEntitiesCount() > 0)
                raidService.Value.FinalizeRaid();
        }
    }
}