using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleRelationEffectResetVisualSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelationEffectsComp> effectsPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag>,
            Exc<AwaitingVisualsTag>> filter = default;

        private readonly EcsFilterInject<
            Inc<RelationEffectsComp>,
            Exc<DeadTag>> subjectsFilter = default;


        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var turnEntity in filter.Value)
            {
                // not related to the current turn but broadcast to all effect holders:
                foreach (var subjectEntity in subjectsFilter.Value)
                {
                    ref var effectsComp = ref effectsPool.Value.Get(subjectEntity);

                    ref var resetRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelEffectResetVisualsInfo>(turnEntity);
                    resetRelEffectVisualsInfo.SubjectEntity = world.PackEntityWithWorld(subjectEntity);
                    resetRelEffectVisualsInfo.CurrentEffects = effectsComp.CurrentEffectsInfo;
                }
            }
        }
    }
}
