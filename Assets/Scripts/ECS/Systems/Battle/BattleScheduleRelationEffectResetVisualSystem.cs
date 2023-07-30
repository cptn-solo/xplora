using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleRelationEffectResetVisualSystem<T> : BaseEcsSystem
        where T : struct, IPackedWithWorldRef
    {
        private readonly EcsPoolInject<T> subjectRefPool = default;
        private readonly EcsPoolInject<RelationEffectsComp> effectsPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag, T>,
            Exc<AwaitingVisualsTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var turnEntity in filter.Value)
            {
                ref var subjectRef = ref subjectRefPool.Value.Get(turnEntity);

                if (!subjectRef.Packed.Unpack(out _, out var subjectEntity))
                    throw new Exception("No Subject entity (attacker or target)");

                ref var effectsComp = ref effectsPool.Value.Get(subjectEntity);

                ref var resetRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelEffectResetVisualsInfo>(turnEntity);
                resetRelEffectVisualsInfo.SubjectEntity = subjectRef.Packed;
                resetRelEffectVisualsInfo.CurrentEffects = effectsComp.CurrentEffectsInfo;
            }
        }
    }
}
