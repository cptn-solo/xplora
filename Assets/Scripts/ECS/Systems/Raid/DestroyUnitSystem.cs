using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class DestroyUnitSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EntityViewRef<Hero>> unitPool = default;

        private readonly EcsFilterInject<
            Inc<DestroyTag, EntityViewRef<Hero>>> destroyTagFilter = default;

        public override void RunIfActive(IEcsSystems systems)
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