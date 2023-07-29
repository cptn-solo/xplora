using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignAttackerEffectsSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<SkippedTag> skippedTagPool = default;
        private readonly EcsPoolInject<AttackerEffectsTag> attackerEffectsTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnPool = default;
        private readonly EcsPoolInject<ActiveEffectComp> activeEffectPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter = default;
        private readonly EcsFilterInject<Inc<ActiveEffectComp>> activeEffectsFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                AssignAttackerEffects(entity);
        }

        private void AssignAttackerEffects(int turnEntity)
        {
            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

            foreach (var effentity in activeEffectsFilter.Value)
            {
                ref var activeEffectComp = ref activeEffectPool.Value.Get(effentity);

                if (!activeEffectComp.Subject.EqualsTo(attackerRef.Packed))
                    continue;

                if (activeEffectComp.SkipTurn)
                {
                    turnInfo.State = TurnState.TurnSkipped;
                    
                    if (!skippedTagPool.Value.Has(turnEntity))
                        skippedTagPool.Value.Add(turnEntity);
                }

                if (!attackerEffectsTagPool.Value.Has(turnEntity))
                    attackerEffectsTagPool.Value.Add(turnEntity);
            }
        }
    }
}
