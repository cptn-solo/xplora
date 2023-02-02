using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployUnitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<UnitComp> unitPool;
        private readonly EcsPoolInject<ProduceTag> produceTagPool;

        private readonly EcsFilterInject<Inc<ProduceTag, HeroComp, FieldCellComp>> produceTagFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in produceTagFilter.Value)
            {
                ref var heroComp = ref heroPool.Value.Get(entity);
                ref var cellComp = ref cellPool.Value.Get(entity);

                DeployWorldUnit callback = playerPool.Value.Has(entity) ?
                    raidService.Value.PlayerDeploymentCallback :
                    raidService.Value.OpponentDeploymentCallback;

                ref var unitComp = ref unitPool.Value.Add(entity);
                unitComp.Unit = callback(cellComp.CellIndex, heroComp.Hero);

                produceTagPool.Value.Del(entity);
            }
            
        }
    }
}