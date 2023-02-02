using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainGenerationSystem : IEcsRunSystem
    {
        private EcsPoolInject<ProduceTag> produceTagPool;
        private EcsFilterInject<Inc<WorldComp, ProduceTag>> worlFilter;

        private EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var worldEntity in worlFilter.Value)
            {
                worldService.Value.GenerateTerrain();

                produceTagPool.Value.Del(worldEntity);
            }
        }
    }
}