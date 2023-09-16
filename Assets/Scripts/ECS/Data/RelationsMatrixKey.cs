using Leopotam.EcsLite;
using System;

namespace Assets.Scripts.ECS.Data
{
    public class RelationsMatrixKey : Tuple<EcsPackedEntityWithWorld, EcsPackedEntityWithWorld>
    {
        public RelationsMatrixKey(EcsPackedEntityWithWorld item1, EcsPackedEntityWithWorld item2) : base(item1, item2)
        {
        }

        public override bool Equals(object obj)
        {
            // mirrors equal condition as this is a relative keys so only values matter, not their positions
            return obj is RelationsMatrixKey key &&
                (Item1.EqualsTo(key.Item1) && Item2.EqualsTo(key.Item2) ||
                Item1.EqualsTo(key.Item2) && Item2.EqualsTo(key.Item1));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2);
        }
    }
}
