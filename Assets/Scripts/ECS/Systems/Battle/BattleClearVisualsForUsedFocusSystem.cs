using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearVisualsForUsedFocusSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<UpdateTag<RelationEffectInfo>> updatePool = default;
        private readonly EcsPoolInject<GarbageTag> garbagePool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<TransformRef<AimTargetTag>> aimIconPool = default;

        private readonly EcsFilterInject<Inc<UsedFocusEntityTag, EffectFocusComp>> usedFocusFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var ufEntity in usedFocusFilter.Value)
            {
                if (!garbagePool.Value.Has(ufEntity))
                    garbagePool.Value.Add(ufEntity);

                ref var focusComp = ref focusPool.Value.Get(ufEntity);
                if (!focusComp.Actor.Unpack(out var world, out var entity))
                    throw new Exception("Stale Focus Actor");

                if (aimIconPool.Value.Has(ufEntity))
                {
                    ref var iconView = ref aimIconPool.Value.Get(ufEntity);
                    GameObject.Destroy(iconView.Transform.gameObject);
                }

                ref var relEffects = ref relEffectsPool.Value.Get(entity);                
                relEffects.RemoveByType(focusComp.EffectKey.RelationsEffectType, out var decrement);
                
                if (decrement != null)
                {
                    foreach (var item in decrement)
                        if (item.Unpack(out var origWorld, out var origEntity))
                            origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, origEntity);
                }

                if (!updatePool.Value.Has(entity))
                    updatePool.Value.Add(entity);
            }
        }
    }
}
