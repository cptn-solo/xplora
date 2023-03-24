using System;

namespace Assets.Scripts.Data
{
    public class RelationStateValue : Tuple<RelationState, int>
    {
        public RelationState State => Item1;
        public int Value => Item2;

        public RelationStateValue(RelationState item1, int item2) : base(item1, item2)
        {
        }
        
    }
}