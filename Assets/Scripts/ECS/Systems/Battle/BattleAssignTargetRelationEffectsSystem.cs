using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignTargetRelationEffectsSystem : 
        BattleAssignRelationEffectsSystem<TargetRef>
    {
        protected override RelationSubjectState SubjectState =>
            RelationSubjectState.BeingAttacked;

    }
}
