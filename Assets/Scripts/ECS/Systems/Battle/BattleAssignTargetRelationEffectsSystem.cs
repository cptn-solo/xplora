using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignTargetRelationEffectsSystem : 
        BattleAssignRelationEffectsSystem<TargetRef>
    {

        protected readonly EcsPoolInject<AttackerRef> attackerRefPool = default;

        protected override RelationSubjectState SubjectState =>
            RelationSubjectState.BeingAttacked;

        protected override void OnEffectInstanceReady(ref EffectInstanceInfo effect, int turnEntity)
        {
            if (effect.Rule.EffectType switch { 
                RelationsEffectType.AlgoRevenge => true,
                RelationsEffectType.AlgoTarget => true,
                _ => false
                })
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
                effect.EffectFocus = attackerRef.Packed;
            }
        }
    }
}
