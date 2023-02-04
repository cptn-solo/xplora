using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyPoiSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PoiRefComp> poiRefPool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;

        private readonly EcsFilterInject<Inc<DestroyTag, PoiRefComp>> destroyTagFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);
                var destroyedPoi = poiRef.PoiRef;
                poiRef.PoiRef = null;

                poiRefPool.Value.Del(entity);
                destroyTagPool.Value.Del(entity);

                worldService.Value.PoiDestroyCallback(destroyedPoi);
            }

        }
    }
}