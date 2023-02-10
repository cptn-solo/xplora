using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class DestroyPoiSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PoiRef> poiRefPool;

        private readonly EcsFilterInject<Inc<DestroyTag, PoiRef>> destroyTagFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);
                var destroyedPoi = poiRef.Poi;
                poiRef.Poi = null;

                poiRefPool.Value.Del(entity);

                worldService.Value.PoiDestroyCallback(destroyedPoi);
            }

        }
    }
}