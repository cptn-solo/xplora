using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class TerrainDestructionSystem : IEcsRunSystem
    {
        private EcsPoolInject<DestroyTag> destroyTagPool;
        private EcsPoolInject<VisibilityRef> visibilityRefPool;

        private EcsFilterInject<Inc<VisibilityRef>> visibilityFilter;
        private EcsFilterInject<Inc<WorldComp, DestroyTag>> worldFilter;

        private EcsCustomInject<WorldService> worldService;

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