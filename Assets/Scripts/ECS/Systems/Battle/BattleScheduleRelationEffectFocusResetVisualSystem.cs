using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleScheduleRelationEffectFocusResetVisualSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;

        private readonly EcsFilterInject<Inc<UsedFocusEntityTag, EffectFocusComp>> usedFocusFilter = default;
        
        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, CompletedTurnTag, ScheduleVisualsTag>,
            Exc<AwaitingVisualsTag>> turnFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var turnEntity in turnFilter.Value)
            {
                // not related to turn, broadcast:
                foreach (var focusEntity in usedFocusFilter.Value)
                {
                    ref var focusComp = ref focusPool.Value.Get(focusEntity);
                    if (!focusComp.Focused.Unpack(out var world, out var focusedEntity))
                        throw new Exception("Stale focused entity");

                    ref var resetRelEffectVisualsInfo = ref world.ScheduleSceneVisuals<RelationEffectsFocusResetInfo>(turnEntity);
                    resetRelEffectVisualsInfo.SubjectEntity = focusComp.Actor;
                    resetRelEffectVisualsInfo.FocusEntity = focusComp.Actor;
                }
            }
        }
    }
}
