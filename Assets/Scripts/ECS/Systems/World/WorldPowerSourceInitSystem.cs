using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldPowerSourceInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;
        private readonly EcsPoolInject<WorldComp> worldPool;

        private readonly EcsFilterInject<Inc<WorldComp>> worldFilter;

        private readonly EcsPoolInject<PowerSourceComp> pSourcePool;

        public void Init(IEcsSystems systems)
        {
            foreach(var worldEntity in worldFilter.Value)
            {
                ref var worldComp = ref worldPool.Value.Get(worldEntity);
                for (int i = 0; i < worldComp.PowerSourceCount; i++)
                    pSourcePool.Value.Add(ecsWorld.Value.NewEntity());

            }

        }
    }
}