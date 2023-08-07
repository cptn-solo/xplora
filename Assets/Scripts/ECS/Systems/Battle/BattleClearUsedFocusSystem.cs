﻿using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearUsedFocusSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<UsedFocusEntityTag> usedFocusPool = default;
        
        private readonly EcsFilterInject<Inc<UsedFocusEntityTag, EffectFocusComp>> usedFocusFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var ufEntity in usedFocusFilter.Value)
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
                usedFocusPool.Value.Del(ufEntity);
            }
        }
    }
}
