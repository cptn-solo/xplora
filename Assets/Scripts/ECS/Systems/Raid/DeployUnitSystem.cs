using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;
using System;
using Assets.Scripts.Data;
using Assets.Scripts.World;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployUnitSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<HeroComp> heroPool = default;
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;
        private readonly EcsPoolInject<PlayerComp> playerPool = default;
        private readonly EcsPoolInject<EntityViewRef<Hero>> unitPool = default;

        private readonly EcsPoolInject<EntityViewFactoryRef<Hero>> factoryPool = default;
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<Hero>>> factoryFilter = default;

        private readonly EcsFilterInject<Inc<ProduceTag, HeroComp, FieldCellComp>> produceTagFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            bool producing = false;

            foreach (var factoryEntity in factoryFilter.Value)
            {
                ref var factoryRef = ref factoryPool.Value.Get(factoryEntity);

                foreach (var entity in produceTagFilter.Value)
                {
                    producing = true;

                    ref var heroComp = ref heroPool.Value.Get(entity);

                    if (!heroComp.Packed.Unpack(out var libWorld, out var libEntity))
                        throw new Exception("No Hero config");

                    ref var cellComp = ref cellPool.Value.Get(entity);

                    var isPlayerUnit = playerPool.Value.Has(entity);
                    var packed = ecsWorld.Value.PackEntityWithWorld(entity);
                    var entityView = (Unit)factoryRef.FactoryRef(packed);

                    DeployWorldUnit callback = isPlayerUnit ?
                        raidService.Value.PlayerDeploymentCallback :
                        raidService.Value.OpponentDeploymentCallback;

                    callback(entityView, cellComp.CellIndex);

                    var hero = libWorld.GetPool<Hero>().Get(libEntity);
                    entityView.SetHero(hero, isPlayerUnit);

                    ref var unitRef = ref unitPool.Value.Add(entity);
                    unitRef.EntityView = entityView;
                }
            }
            
            if (producing)
                raidService.Value.OnUnitsSpawned();
            
        }
    }
}