﻿using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Drains power from source
    /// </summary>
    public class DrainSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool = default;
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;
        private readonly EcsPoolInject<OutOfPowerTag> oopTagPool = default;
        private readonly EcsPoolInject<DrainComp> drainPool = default;

        private readonly EcsFilterInject<
            Inc<DrainComp, PowerComp>,
            Exc<OutOfPowerTag>> drainFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in drainFilter.Value)
            {
                ref var drainComp = ref drainPool.Value.Get(entity);
                ref var powerComp = ref powerPool.Value.Get(entity);
                powerComp.CurrentValue -= drainComp.Value;

                if (powerComp.CurrentValue <= 0)
                    oopTagPool.Value.Add(entity);

                if (!updateTagPool.Value.Has(entity))
                    updateTagPool.Value.Add(entity);
            }
        }
    }
}