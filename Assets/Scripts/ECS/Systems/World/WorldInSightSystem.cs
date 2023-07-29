using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldInSightSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<ProduceTag> produceTagPool = default;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool = default;

        private readonly EcsFilterInject<
            Inc<VisibilityUpdateTag, FieldCellComp, VisibleTag, POIComp, WorldPoiTag>,
            Exc<EntityViewRef<bool>, ProduceTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                produceTagPool.Value.Add(entity);
                if (destroyTagPool.Value.Has(entity))
                    destroyTagPool.Value.Del(entity);
            }
        }
    }
}