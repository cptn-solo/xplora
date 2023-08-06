using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Schedules turn visualization calls for relation effects, 
    /// all pending effects visualized
    /// </summary>
    public class BattleScheduleRelationEffectVisualSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelationEffectsPendingComp> pendingPool = default;
        private readonly EcsPoolInject<TransformRef<VisualsTransformTag>> transformRefPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag>,
            Exc<AwaitingVisualsTag>> filter = default;

        private readonly EcsFilterInject<
            Inc<RelationEffectsPendingComp>> scheduleFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var turnEntity in filter.Value)
            {
                foreach (var scheduleEntity in scheduleFilter.Value)
                {
                    ref var pendingRelEffectsComp = ref pendingPool.Value.Get(scheduleEntity);

                    ref var castRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelEffectCastVisualsInfo>(turnEntity);
                    castRelEffectVisualsInfo.SubjectEntity = pendingRelEffectsComp.EffectTarget;
                    castRelEffectVisualsInfo.EffectInfo = pendingRelEffectsComp.EffectInfo;

                    if (!pendingRelEffectsComp.EffectSource.Unpack(out _, out var effectSourceEntity))
                        throw new Exception("Stale effect source entity");

                    if (!pendingRelEffectsComp.EffectTarget.Unpack(out _, out var effectTargetEntity))
                        throw new Exception("Stale effect target entity");

                    var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(effectSourceEntity);
                    var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(effectTargetEntity);
                    Debug.Log($"Visual move from {nameSource} to {nameSubject}");

                    // rare if possible case when effect source view is unavailable (dead hero?)
                    var viewRefEntity = transformRefPool.Value.Has(effectSourceEntity) ?
                        effectSourceEntity : (transformRefPool.Value.Has(effectTargetEntity) ?
                        effectTargetEntity : -1);

                    if (effectTargetEntity == -1)
                        throw new Exception("No view for the effect animation start point");

                    ref var viewRef = ref transformRefPool.Value.Get(viewRefEntity);
                    castRelEffectVisualsInfo.SourceTransform = viewRef.Transform;

                    pendingPool.Value.Del(scheduleEntity);

                }
            }
        }
    }
}
