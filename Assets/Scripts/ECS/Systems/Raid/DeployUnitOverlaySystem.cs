using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.World;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployUnitOverlaySystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<EntityViewRef<Hero>> unitPool;

        private readonly EcsPoolInject<EntityViewRef<BarsInfo>> overlayPool;

        private readonly EcsPoolInject<EntityViewFactoryRef<BarsInfo>> factoryRefPool;
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<BarsInfo>>> factoryRefFilter;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>, PlayerComp, PowerComp>,
            Exc<EntityViewRef<BarsInfo>>> produceTagFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var factoryRefEntity in factoryRefFilter.Value)
            {
                ref var factoryRef = ref factoryRefPool.Value.Get(factoryRefEntity);

                foreach (var entity in produceTagFilter.Value)
                {
                    ref var unitRef = ref unitPool.Value.Get(entity);

                    var packed = ecsWorld.Value.PackEntityWithWorld(entity);

                    var entityView = (UnitOverlay)factoryRef.FactoryRef(packed);
                    entityView.Attach(unitRef.EntityView.Transform);

                    ref var overlayRef = ref overlayPool.Value.Add(entity);
                    overlayRef.EntityView = entityView;
                }
            }


        }
    }
}