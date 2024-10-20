﻿using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearDeadHeroesRelEffects : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<RelEffectResetPendingTag> resetEffectsPool = default;
        private readonly EcsPoolInject<FocusResetPendingTag> resetFocusPool = default;

        private readonly EcsFilterInject<
            Inc<DeadTag>
            > filter = default;
        
        private readonly EcsFilterInject<
            Inc<EffectFocusComp>> focusFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            foreach (var entity in filter.Value)
            {
                var en = world.SubjectEffectsEntities(entity);
                while (en.MoveNext())
                {
                    // reset focus and effects assigned to the dead:

                    if (resetFocusPool.Value.Has(entity))
                        resetFocusPool.Value.Add(entity);

                    if (!resetEffectsPool.Value.Has(en.Current))
                        resetEffectsPool.Value.Add(en.Current);
                }

                foreach (var focusEntity in focusFilter.Value)
                {
                    // reset focus and effects that were focused on the dead:
                    
                    ref var focus = ref focusPool.Value.Get(focusEntity);
                    if (!focus.Focused.Unpack(out _, out var focusedEntity))
                        throw new Exception("Stale focused entity");

                    if (focusedEntity != entity)
                        continue;

                    if (!focus.EffectEntity.Unpack(out _, out var effectEntity))
                        throw new Exception("Stale effect entity");

                    if (!resetEffectsPool.Value.Has(effectEntity))
                        resetEffectsPool.Value.Add(effectEntity);           
                    
                    if (!resetFocusPool.Value.Has(focusEntity))
                        resetFocusPool.Value.Add(focusEntity);
                }

            }
        }
    }
}
