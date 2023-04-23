using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignAttackerRelationEffectsSystem : 
        BattleAssignRelationEffectsSystem<AttackerRef>
    {
        protected override RelationSubjectState SubjectState =>
            RelationSubjectState.Attacking;
    }
}
