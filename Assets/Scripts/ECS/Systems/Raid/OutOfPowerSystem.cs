using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class OutOfPowerSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<OutOfPowerTag, PlayerComp>> oopFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            if (oopFilter.Value.GetEntitiesCount() > 0)
                raidService.Value.FinalizeRaid();
        }
    }
}