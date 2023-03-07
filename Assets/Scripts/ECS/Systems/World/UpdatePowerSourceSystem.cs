using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateWorldPoiSystem<T> : IEcsRunSystem
        where T: struct
    {
        private readonly EcsPoolInject<PoiRef> poiRefPool;
        private readonly EcsPoolInject<UsedTag> usedTagPool;

        private readonly EcsFilterInject<
            Inc<T, UpdateTag, PoiRef>> updateFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);

                poiRef.Poi.Toggle(!usedTagPool.Value.Has(entity));
            }
        }
    }
}