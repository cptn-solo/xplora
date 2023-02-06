using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Refills power from source
    /// </summary>
    public class RefillSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<RefillComp> refillPool;
        private readonly EcsPoolInject<UpdateTag> updateTagPool;

        private readonly EcsFilterInject<Inc<RefillComp, PowerComp>> refillFilter;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in refillFilter.Value)
            {
                ref var refillComp = ref refillPool.Value.Get(entity);
                ref var powerComp = ref powerPool.Value.Get(entity);

                //TODO: implement partial refill
                powerComp.CurrentValue = powerComp.InitialValue;

                if (!updateTagPool.Value.Has(entity))
                    updateTagPool.Value.Add(entity);
            }
        }
    }
}