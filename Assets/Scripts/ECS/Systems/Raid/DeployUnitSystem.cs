using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;
using UnityEngine.LowLevel;
using System;
using Assets.Scripts.Data;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployUnitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<UnitRef> unitPool;

        private readonly EcsFilterInject<Inc<ProduceTag, HeroComp, FieldCellComp>> produceTagFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            bool producing = false;

            foreach (var entity in produceTagFilter.Value)
            {
                producing = true;

                ref var heroComp = ref heroPool.Value.Get(entity);
                ref var cellComp = ref cellPool.Value.Get(entity);

                var isPlayerUnit = playerPool.Value.Has(entity);

                DeployWorldUnit callback = isPlayerUnit ?
                    raidService.Value.PlayerDeploymentCallback :
                    raidService.Value.OpponentDeploymentCallback;

                if (!heroComp.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero config");

                var hero = libWorld.GetPool<Hero>().Get(libEntity);

                ref var unitRef = ref unitPool.Value.Add(entity);
                unitRef.Unit = callback(cellComp.CellIndex, hero);
            }

            if (producing)
                raidService.Value.OnUnitsSpawned();
            
        }
    }
}