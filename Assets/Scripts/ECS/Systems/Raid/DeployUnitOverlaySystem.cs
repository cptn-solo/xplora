using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Data;
using Assets.Scripts.World;

namespace Assets.Scripts.ECS.Systems
{
    public class DeployUnitOverlaySystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<EntityViewRef<Hero>> unitPool = default;
        private readonly EcsPoolInject<UpdateTag<StrengthTag>> updatePool = default;
        
        private readonly EcsPoolInject<EntityViewRef<UnitInfo>> overlayPool = default;

        private readonly EcsPoolInject<EntityViewFactoryRef<UnitInfo>> factoryRefPool = default;
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<UnitInfo>>> factoryRefFilter = default;

        private readonly EcsFilterInject<
            Inc<ProduceTag, EntityViewRef<Hero>>,
            Exc<EntityViewRef<UnitInfo>>> produceTagFilter = default;

        public override void RunIfActive(IEcsSystems systems)
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

                    if (!updatePool.Value.Has(entity))
                        updatePool.Value.Add(entity);
                }
            }


        }
    }
}