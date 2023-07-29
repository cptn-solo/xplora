﻿using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearUsedRelationsEffectSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectInstanceInfo> pool;
        private readonly EcsFilterInject<
            Inc<EffectInstanceInfo>
            > filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var comp = ref pool.Value.Get(entity);
                if (comp.UsageLeft <= 0)
                    pool.Value.Del(entity);
            }
        }
    }
}
