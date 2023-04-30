using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainDestructionSystem : IEcsRunSystem
    {
        private EcsPoolInject<VisibilityRef> visibilityRefPool = default;

        private EcsFilterInject<Inc<VisibilityRef>> visibilityFilter = default;
        private EcsFilterInject<Inc<WorldComp, DestroyTag>> worldFilter = default;

        private EcsCustomInject<WorldService> worldService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var worldEntity in worldFilter.Value)
            {
                foreach (var visEntity in visibilityFilter.Value)
                {
                    ref var visibilityRef = ref visibilityRefPool.Value.Get(visEntity);
                    visibilityRef.visibility = null;

                    visibilityRefPool.Value.Del(visEntity);
                }
                    

                worldService.Value.DestroyTerrain();
                
            }
        }
    }
}