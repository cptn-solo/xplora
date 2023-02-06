using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class OutOfPowerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<OutOfPowerTag> oopTagPool;

        private readonly EcsFilterInject<Inc<OutOfPowerTag, PowerComp>> oopFilter;


        public void Run(IEcsSystems systems)
        {
            //TODO: mark source as disabled
        }
    }

    /// <summary>
    /// Drains power from source
    /// </summary>
    public class DrainSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<OutOfPowerTag> oopTagPool;
        private readonly EcsPoolInject<DrainComp> drainPool;

        private readonly EcsFilterInject<Inc<DrainComp, PowerComp>> drainFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in drainFilter.Value)
            {
                ref var drainComp = ref drainPool.Value.Get(entity);
                ref var powerComp = ref powerPool.Value.Get(entity);
                powerComp.CurrentValue -= drainComp.Value;

                if (powerComp.CurrentValue <= 0)
                    oopTagPool.Value.Add(entity);

                drainPool.Value.Del(entity);
            }
        }
    }
}