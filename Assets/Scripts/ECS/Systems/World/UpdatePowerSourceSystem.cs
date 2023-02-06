using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdatePowerSourceSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PoiRef> poiRefPool;
        private readonly EcsPoolInject<OutOfPowerTag> oopTagPool;

        private readonly EcsFilterInject<
            Inc<UpdateTag, PowerSourceComp, PoiRef>> updateFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);

                poiRef.Poi.Toggle(!oopTagPool.Value.Has(entity));
            }
        }
    }
}