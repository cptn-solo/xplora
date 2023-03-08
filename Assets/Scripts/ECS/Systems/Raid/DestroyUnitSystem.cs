using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> unitPool;

        private readonly EcsFilterInject<
            Inc<DestroyTag, EntityViewRef<Hero>>> destroyTagFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in destroyTagFilter.Value)
            {
                ref var unitRef = ref unitPool.Value.Get(entity);
                unitRef.EntityView.Destroy();
                unitRef.EntityView = null;

                unitPool.Value.Del(entity);
            }
        }
    }
}