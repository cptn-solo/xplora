using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldInitSystem : IEcsInitSystem
    {
        private EcsWorldInject ecsWorld;

        private EcsCustomInject<WorldService> worldService;

        private EcsPoolInject<WorldComp> worldPool;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();
            worldPool.Value.Add(entity);

            worldService.Value.WorldEntity = ecsWorld.Value.PackEntity(entity);
        }
    }
}