using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class TerrainDestructionSystem : IEcsRunSystem
    {
        private EcsPoolInject<DestroyTag> destroyTagPool;

        private EcsFilterInject<Inc<FieldVisibilityRef>> visibilityFilter;
        private EcsFilterInject<Inc<WorldComp, DestroyTag>> worldFilter;

        private EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var worldEntity in worldFilter.Value)
            {
                foreach (var visEntity in visibilityFilter.Value)
                    destroyTagPool.Value.Add(visEntity);

                worldService.Value.DestroyTerrain();
                
            }
        }
    }
}