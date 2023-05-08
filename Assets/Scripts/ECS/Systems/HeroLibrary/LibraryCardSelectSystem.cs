using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryCardSelectSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<SelectedTag> selectedPool = default;

        private readonly EcsFilterInject<Inc<UpdateTag<SelectedTag>>> filter = default;
        private readonly EcsFilterInject<Inc<SelectedTag>> selectedFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                foreach (var selectedEntity in selectedFilter.Value)
                    selectedPool.Value.Del(selectedEntity);                    

                selectedPool.Value.Add(entity);
            }
        }
    }
}

