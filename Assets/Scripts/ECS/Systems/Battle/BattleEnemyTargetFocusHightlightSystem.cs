using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleEnemyTargetFocusHightlightSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> pool = default;        
        private readonly EcsPoolInject<RelationEffectsFocusPendingComp> pendingPool = default;

        private readonly EcsFilterInject<
            Inc<EffectFocusComp, DraftTag<RelationEffectsFocusPendingComp>>, 
            Exc<RelationEffectsFocusPendingComp>
            > filter = default;
        
        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var focusComp = ref pool.Value.Get(entity);
                if (!focusComp.Focused.Unpack(out var world, out var focusedEntity))
                    throw new Exception("Stale Focused entity");

                if (!focusComp.Actor.Unpack(out _, out var actorEntity))
                    throw new Exception("Stale Focus Actor entity");

                var pendingEntity = world.NewEntity();
                ref var pendingVisual = ref pendingPool.Value.Add(pendingEntity);

                var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(actorEntity);
                var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(focusedEntity);
                Debug.Log($"Pending focus from {nameSource} to {nameSubject}");

                pendingVisual.EffectSource = world.PackEntityWithWorld(actorEntity);
                pendingVisual.EffectTarget = world.PackEntityWithWorld(focusedEntity);
                pendingVisual.EffectType = focusComp.EffectKey.RelationsEffectType;
                pendingVisual.FocusEntity = world.PackEntityWithWorld(entity);
            }
        }
    }
}
