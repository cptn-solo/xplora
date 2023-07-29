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
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag, AttackerRef>,
            Exc<AwaitingVisualsTag>> turnFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var turnEntity in turnFilter.Value)
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

                if (!attackerRef.Packed.Unpack(out var world, out var attackerEntity))
                    throw new Exception("No attacker entity for focus");

                //Debug.Break();
                foreach (var scheduleEntity in scheduleFilter.Value)
                {
                    ref var pendingRelEffectsComp = ref pendingPool.Value.Get(scheduleEntity);

                    if (!pendingRelEffectsComp.EffectSource.Unpack(out _, out var effectSourceEntity))
                        throw new Exception("Stale effect source entity");

                    if (!pendingRelEffectsComp.EffectTarget.Unpack(out _, out var effectTargetEntity))
                        throw new Exception("Stale effect target entity");

                    var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(effectSourceEntity);
                    var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(effectTargetEntity);
                    var nameAttacker = world.ReadValue<NameValueComp<NameTag>, string>(attackerEntity);
                    Debug.Log($"Visual cast focus from {nameSource} to {nameSubject} attacker {nameAttacker}");

                    // checking if the turn's target is focused by the effect:
                    if (!pendingRelEffectsComp.EffectSource.EqualsTo(attackerRef.Packed))
                    {
                        Debug.Log($"Visual cast focus failed");
                        continue;
                    }

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
