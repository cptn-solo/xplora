using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerSystem : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem
    {
        private readonly EcsWorldInject ecsWorld = default;
        private readonly EcsPoolInject<PlayerComp> playerPool = default;
        private readonly EcsPoolInject<TeamComp> teamPool = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Init(IEcsSystems systems)
        {
            var playerEntity = ecsWorld.Value.NewEntity();
            worldService.Value.PlayerEntity = ecsWorld.Value.PackEntity(playerEntity);

            ref var playerComp = ref playerPool.Value.Add(playerEntity);

            ref var teamComp = ref teamPool.Value.Add(playerEntity);
        }
        public void Run(IEcsSystems systems)
        {
        }
        public void Destroy(IEcsSystems systems)
        {
        }
    }
}