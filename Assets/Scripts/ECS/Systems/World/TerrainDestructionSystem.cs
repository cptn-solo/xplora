using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class TerrainDestructionSystem : IEcsRunSystem
    {
        private EcsPoolInject<DestroyTag> destroyTagPool;

        private EcsFilterInject<Inc<WorldComp, DestroyTag>> worlFilter;

        private EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var worldEntity in worlFilter.Value)
            {
                worldService.Value.DestroyTerrain();

                destroyTagPool.Value.Del(worldEntity);
            }
        }
    }
}