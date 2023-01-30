using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldSystem : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem
    {
        private readonly EcsFilterInject<Inc<UnitComp>> unitsFilter = default;
        private readonly EcsFilterInject<Inc<POIComp>> poiFilter = default;

        public void Init(IEcsSystems systems)
        {
        }
        public void Run(IEcsSystems systems)
        {
        }
        public void Destroy(IEcsSystems systems)
        {
        }
    }
    public class RaidSystem : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem
    {
        private readonly EcsPoolInject<UnitComp> unitsPool = default;

        public void Init(IEcsSystems systems)
        {
        }
        public void Run(IEcsSystems systems)
        {
        }
        public void Destroy(IEcsSystems systems)
        {
        }
    }

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

    /// <summary>
    /// Links cell to visitor
    /// </summary>
    public class VisitSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {            
        }
    }

    /// <summary>
    /// Unlinks cell from visitor
    /// </summary>
    public class LeaveSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
        }
    }

    /// <summary>
    /// Refills power from source
    /// </summary>
    public class RefillSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
        }
    }

    /// <summary>
    /// Drains power from source
    /// </summary>
    public class DrainSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
        }
    }

    public class RefSystem : IEcsPreInitSystem, IEcsInitSystem, IEcsRunSystem, IEcsPostRunSystem, IEcsDestroySystem, IEcsPostDestroySystem
    {
        public void PreInit(IEcsSystems systems)
        {
            // Будет вызван один раз в момент работы IEcsSystems.Init() и до срабатывания IEcsInitSystem.Init() у всех систем.
        }

        public void Init(IEcsSystems systems)
        {
            // Будет вызван один раз в момент работы IEcsSystems.Init() и после срабатывания IEcsPreInitSystem.PreInit() у всех систем.
        }

        public void Run(IEcsSystems systems)
        {
            // Будет вызван один раз в момент работы IEcsSystems.Run().
        }

        public void PostRun(IEcsSystems systems)
        {
            // Будет вызван один раз в момент работы IEcsSystems.Run() после срабатывания IEcsRunSystem.Run() у всех систем.
        }

        public void Destroy(IEcsSystems systems)
        {
            // Будет вызван один раз в момент работы IEcsSystems.Destroy() и до срабатывания IEcsPostDestroySystem.PostDestroy() у всех систем.
        }

        public void PostDestroy(IEcsSystems systems)
        {
            // Будет вызван один раз в момент работы IEcsSystems.Destroy() и после срабатывания IEcsDestroySystem.Destroy() у всех систем.
        }
    }
}