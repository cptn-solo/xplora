using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyVisibilityRefsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldVisibilityRef> visibilityRefPool;
        private readonly EcsFilterInject<Inc<DestroyTag, FieldVisibilityRef>> destroyTagFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var visibilityRef = ref visibilityRefPool.Value.Get(entity);
                visibilityRef.visibility = null;

                visibilityRefPool.Value.Del(entity);
            }
        }
    }
}