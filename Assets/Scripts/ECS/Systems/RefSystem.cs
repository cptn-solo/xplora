using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldSystem : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem
    {
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
        public void Init(IEcsSystems systems)
        {
            // spawn units
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