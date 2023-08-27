using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearFocusSystem<T> : BaseEcsSystem where T : struct
    {
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<T> resetFocusPool = default;
        
        private readonly EcsFilterInject<Inc<T, EffectFocusComp>> resetFocusFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var ufEntity in resetFocusFilter.Value)
            {
                ref var focusComp = ref focusPool.Value.Get(ufEntity);
                if (!focusComp.Actor.Unpack(out var world, out var entity))
                    throw new Exception("Stale Focus Actor");

                world.RemoveRelEffectByType(entity, focusComp.EffectKey.RelationsEffectType, out var decrement);
                
                if (decrement != null)
                {
                    foreach (var item in decrement)
                        if (item.Unpack(out var origWorld, out var origEntity))
                            origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, origEntity);
                }

                focusPool.Value.Del(ufEntity);
                resetFocusPool.Value.Del(ufEntity);
            }
        }
    }
}
