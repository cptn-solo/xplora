using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldOutOfSightSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<DestroyTag> destroyTagPool = default;

        private readonly EcsFilterInject<
            Inc<VisibilityUpdateTag, FieldCellComp, EntityViewRef<bool>, WorldPoiTag>,
            Exc<VisibleTag, DestroyTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                destroyTagPool.Value.Add(entity);
        }
    }
}