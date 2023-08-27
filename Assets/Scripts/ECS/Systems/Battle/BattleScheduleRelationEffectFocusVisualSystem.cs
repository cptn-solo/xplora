using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleRelationEffectFocusVisualSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelationEffectsFocusPendingComp> pendingPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TransformRef<VisualsTransformTag>> viewRefPool = default;
        
        private readonly EcsFilterInject<
            Inc<RelationEffectsFocusPendingComp>> scheduleFilter = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag>,
            Exc<AwaitingVisualsTag>> turnFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var turnEntity in turnFilter.Value)
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

                //Debug.Break();
                foreach (var scheduleEntity in scheduleFilter.Value)
                {
                    ref var pendingRelEffectsComp = ref pendingPool.Value.Get(scheduleEntity);

                    if (!pendingRelEffectsComp.EffectSource.Unpack(out var world, out var effectSourceEntity))
                        throw new Exception("Stale effect source entity");

                    if (!pendingRelEffectsComp.EffectTarget.Unpack(out _, out var effectTargetEntity))
                        throw new Exception("Stale effect target entity");

                    if (!pendingRelEffectsComp.FocusEntity.Unpack(out _, out var focusedEntity))
                        throw new Exception("Stale effect target entity");

                    var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(effectSourceEntity);
                    var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(effectTargetEntity);
                    var nameFocused = world.ReadValue<NameValueComp<NameTag>, string>(focusedEntity);
                    Debug.Log($"Visual cast focus from: {nameSource} to: {nameSubject}, focused: {nameFocused}");

                    ref var castRelEffectFocusVisualsInfo = ref world.ScheduleSceneVisuals<RelationEffectsFocusCastInfo>(turnEntity);

                    // NB: attaching focus to the revenge/target actor, not to the focused target (so target icon lives in the attacker, not the victim):
                    castRelEffectFocusVisualsInfo.SubjectEntity = pendingRelEffectsComp.EffectSource;


                    if (!viewRefPool.Value.Has(effectSourceEntity) || !viewRefPool.Value.Has(effectTargetEntity))
                        continue;

                    ref var sourceViewRef = ref viewRefPool.Value.Get(effectSourceEntity);
                    castRelEffectFocusVisualsInfo.SourceTransform = sourceViewRef.Transform;
                    
                    ref var targetViewRef = ref viewRefPool.Value.Get(effectTargetEntity);
                    // var anchor = viewRef.EntityView.Transform;
                    castRelEffectFocusVisualsInfo.TargetTransform = targetViewRef.Transform;

                    castRelEffectFocusVisualsInfo.FocusInfo = new BundleIconInfo
                    {
                        Icon = BundleIcon.AimTarget,
                        IconColor = pendingRelEffectsComp.EffectType == RelationsEffectType.AlgoRevenge ?
                            Color.blue : Color.Lerp(Color.red, Color.yellow, .5f),
                    };

                    pendingPool.Value.Del(scheduleEntity);
                }
            }

        }
    }
}
