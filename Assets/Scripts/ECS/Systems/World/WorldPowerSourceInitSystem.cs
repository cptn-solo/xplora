using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldPowerSourceInitSystem : IEcsInitSystem
    {
        private const int pSourceCount = 3;

        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<PowerSourceComp> pSourcePool;

        public void Init(IEcsSystems systems)
        {
            for (int i = 0; i < pSourceCount; i++)
                pSourcePool.Value.Add(ecsWorld.Value.NewEntity());
        }
    }
}