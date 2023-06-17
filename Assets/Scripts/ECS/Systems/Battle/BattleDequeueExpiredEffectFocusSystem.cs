using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueExpiredEffectFocusSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<RetiredTag> retiredTagPool = default;
        private readonly EcsPoolInject<GarbageTag> garbageTagPool = default;
        
        private readonly EcsFilterInject<Inc<EffectFocusComp>> filter = default;
        private readonly EcsFilterInject<Inc<BattleRoundInfo, GarbageTag>> completeRoundFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value) {
                ref var focus = ref focusPool.Value.Get(entity);
                if (!focus.Focused.Unpack(out var world, out var focusedEntity))
                    throw new Exception("Stale Focused Ref");

                if (retiredTagPool.Value.Has(focusedEntity) &&
                    !garbageTagPool.Value.Has(entity))
                    garbageTagPool.Value.Add(entity);

                foreach (var roundEntity in completeRoundFilter.Value)
                {
                    ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);
                    if (roundInfo.Round <= focus.EndRound &&
                        !garbageTagPool.Value.Has(entity))
                        garbageTagPool.Value.Add(entity);
                }
            }
        }
    }
}
