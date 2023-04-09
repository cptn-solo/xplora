using System;

namespace Assets.Scripts.Data
{
    public class RelationEffectLibraryKey : Tuple<int, RelationSubjectState, RelationState>
    {
        public int SubjectHeroConfigId => Item1;
        public RelationSubjectState SubjectState => Item2;
        public RelationState SubjectRelativeState => Item3;

        public RelationEffectLibraryKey(
            int subjectHeroConfigId, 
            RelationSubjectState subjectState, 
            RelationState subjectRelativeState) : base(
                subjectHeroConfigId, 
                subjectState, 
                subjectRelativeState)
        {
        }
    }
}