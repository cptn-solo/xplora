using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldVeilFieldCellsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<VisibilityUpdateTag> visibilityUpdateTagPool = default;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool = default;
        private readonly EcsPoolInject<VisibleTag> visibleTagPool = default;

        private readonly EcsFilterInject<Inc<FieldCellComp, VisibilityRef, VeilCellsTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var visibilityRef = ref visibilityRefPool.Value.Get(entity);
                visibilityRef.visibility.DecreaseVisibility();

                if (visibleTagPool.Value.Has(entity))
                    visibleTagPool.Value.Del(entity);

                if (!visibilityUpdateTagPool.Value.Has(entity))
                    visibilityUpdateTagPool.Value.Add(entity);
            }
        }
    }
}