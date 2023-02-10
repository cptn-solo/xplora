using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
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