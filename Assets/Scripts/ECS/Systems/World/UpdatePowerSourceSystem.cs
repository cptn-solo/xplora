using Assets.Scripts.ECS.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateWorldPoiSystem<T> : BaseEcsSystem
        where T: struct
    {
        private readonly EcsPoolInject<EntityViewRef<bool>> poiRefPool = default;
        private readonly EcsPoolInject<UsedTag> usedTagPool = default;

        private readonly EcsFilterInject<
            Inc<T, UpdateTag, EntityViewRef<bool>>> updateFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var poiRef = ref poiRefPool.Value.Get(entity);
                var entityView = (POI)poiRef.EntityView;
                entityView.Toggle(!usedTagPool.Value.Has(entity));
            }
        }
    }
}