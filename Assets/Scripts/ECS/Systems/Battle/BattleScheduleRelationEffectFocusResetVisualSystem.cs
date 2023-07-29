using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleRelationEffectFocusResetVisualSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;

        private readonly EcsFilterInject<Inc<UsedFocusEntityTag, EffectFocusComp>> usedFocusFilter = default;
        
        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag, AttackerRef>,
            Exc<AwaitingVisualsTag>> turnFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var turnEntity in turnFilter.Value)
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

                if (!attackerRef.Packed.Unpack(out var world, out var attackerEntity))
                    throw new Exception("No attacker entity for focus reset");

                foreach (var focusEntity in usedFocusFilter.Value)
                {
                    ref var focusComp = ref focusPool.Value.Get(focusEntity);
                    if (!focusComp.Focused.Unpack(out _, out var focusedEntity))
                        throw new Exception("Staled focused entity");

                    if (!focusComp.Actor.EqualsTo(attackerRef.Packed))
                        continue; // Irrelevant

                    ref var resetRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelationEffectsFocusResetInfo>(turnEntity);
                    resetRelEffectVisualsInfo.SubjectEntity = attackerRef.Packed;
                    resetRelEffectVisualsInfo.FocusEntity = attackerRef.Packed;
                }
            }
        }
    }
}
