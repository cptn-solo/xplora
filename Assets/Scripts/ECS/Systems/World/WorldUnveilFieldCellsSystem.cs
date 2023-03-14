using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldUnveilFieldCellsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<VisibilityUpdateTag> visibilityUpdateTagPool = default;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool = default;
        private readonly EcsPoolInject<VisibleTag> visibleTagPool = default;
        private readonly EcsPoolInject<ExploredTag> exploredTagPool = default;

        private readonly EcsFilterInject<Inc<FieldCellComp, VisibilityRef, UnveilCellsTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {                

                ref var visibilityRef = ref visibilityRefPool.Value.Get(entity);
                visibilityRef.visibility.IncreaseVisibility();

                if (!visibleTagPool.Value.Has(entity))
                    visibleTagPool.Value.Add(entity);

                if (!exploredTagPool.Value.Has(entity))
                    exploredTagPool.Value.Add(entity); // will stay explored after hide

                if (!visibilityUpdateTagPool.Value.Has(entity))
                    visibilityUpdateTagPool.Value.Add(entity);
            }
        }
    }
}