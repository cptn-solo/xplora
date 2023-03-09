using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldVeilFieldCellsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<VisibilityUpdateTag> visibilityUpdateTagPool;
        private readonly EcsPoolInject<VisibilityRef> visibilityRefPool;
        private readonly EcsPoolInject<VisibleTag> visibleTagPool;

        private readonly EcsFilterInject<Inc<FieldCellComp, VisibilityRef, VeilCellsTag>> filter;

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