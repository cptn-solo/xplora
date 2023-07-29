using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueExpiredEffectFocusSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<TransformRef<AimTargetTag>> aimTagPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<RetiredTag> retiredTagPool = default;
        
        private readonly EcsFilterInject<Inc<EffectFocusComp>> filter = default;
        private readonly EcsFilterInject<Inc<BattleRoundInfo, GarbageTag>> completeRoundFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value) {

                ref var focus = ref focusPool.Value.Get(entity);
                bool expire = false;

                if (!focus.Focused.Unpack(out var world, out var focusedEntity))
                    throw new Exception("Stale Focused Ref");

                if (!focus.Actor.Unpack(out _, out var actorEntity))
                    throw new Exception("Stale Focus Actor Ref");

                if (retiredTagPool.Value.Has(focusedEntity))
                    expire = true;


                foreach (var roundEntity in completeRoundFilter.Value)
                {
                    ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);
                    if (roundInfo.Round <= focus.EndRound)
                        expire = true;
                }

                if (expire)
                {
                    focusPool.Value.Del(entity);

                    if (aimTagPool.Value.Has(actorEntity))
                        aimTagPool.Value.Del(actorEntity);
                }


            }
        }
    }
}
