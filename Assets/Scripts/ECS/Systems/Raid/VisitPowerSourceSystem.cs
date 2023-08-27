using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitPowerSourceSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;
        private readonly EcsPoolInject<RefillComp> refillPool = default;
        private readonly EcsPoolInject<DrainComp> draindPool = default;

        private readonly EcsFilterInject<Inc<StaminaComp, VisitedComp<PowerSourceComp>>> visitFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach(var entity in visitFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Get(entity);

                // refill for player:
                ref var refillComp = ref refillPool.Value.Add(entity);
                refillComp.Value = 10;

                // cancel default drain:
                if (draindPool.Value.Has(entity))
                    draindPool.Value.Del(entity);
            }
        }
    }
}