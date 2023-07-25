using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Schedules turn visualization calls for relation effects, attacker/target heroes 
    /// processed separately as there are some times no pair but only one of them
    /// </summary>
    /// <typeparam name="T">AttackerRef or TargetRef</typeparam>
    public class BattleScheduleRelationEffectVisualSystem<T> : IEcsRunSystem 
        where T : struct, IPackedWithWorldRef
    {
        private readonly EcsPoolInject<T> subjectRefPool = default;
        private readonly EcsPoolInject<RelationEffectsPendingComp> pendingPool = default;
        private readonly EcsPoolInject<TransformRef<VisualsTransformTag>> transformRefPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag, T>,
            Exc<AwaitingVisualsTag>> filter = default;

        private readonly EcsFilterInject<
            Inc<RelationEffectsPendingComp>> scheduleFilter = default;

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var turnEntity in filter.Value)
            {
                ref var subjectRef = ref subjectRefPool.Value.Get(turnEntity);

                if (!subjectRef.Packed.Unpack(out _, out var subjectEntity))
                    throw new Exception("No Subject entity (attacker or target)");

                ref var resetRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelEffectResetVisualsInfo>(turnEntity);
                resetRelEffectVisualsInfo.SubjectEntity = subjectRef.Packed;

                foreach (var scheduleEntity in scheduleFilter.Value)
                {
                    ref var pendingRelEffectsComp = ref pendingPool.Value.Get(scheduleEntity);
                    
                    if (!pendingRelEffectsComp.EffectTarget.EqualsTo(subjectRef.Packed))
                        continue;

                    ref var castRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelEffectCastVisualsInfo>(turnEntity);
                    castRelEffectVisualsInfo.SubjectEntity = subjectRef.Packed;
                    castRelEffectVisualsInfo.EffectInfo = pendingRelEffectsComp.EffectInfo;

                    if (!pendingRelEffectsComp.EffectSource.Unpack(out _, out var effectSourceEntity))
                        throw new Exception("Stale effect source entity");

                    var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(effectSourceEntity);
                    var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(subjectEntity);
                    Debug.Log($"Visual move from {nameSource} to {nameSubject}");

                    // rare if possible case when effect source view is unavailable (dead hero?)
                    var viewRefEntity = transformRefPool.Value.Has(effectSourceEntity) ?
                        effectSourceEntity : (transformRefPool.Value.Has(subjectEntity) ?
                        subjectEntity : -1);

                    if (subjectEntity == -1)
                        throw new Exception("No view for the effect animation start point");

                    ref var viewRef = ref transformRefPool.Value.Get(viewRefEntity);
                    castRelEffectVisualsInfo.SourceTransform = viewRef.Transform;

                }
            }
        }
    }
}
