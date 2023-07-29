using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class OutOfPowerSystem : BaseEcsSystem
    {
        private readonly EcsFilterInject<Inc<OutOfPowerTag, PlayerComp>> oopFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (oopFilter.Value.GetEntitiesCount() > 0)
                raidService.Value.FinalizeRaid();
        }
    }
}