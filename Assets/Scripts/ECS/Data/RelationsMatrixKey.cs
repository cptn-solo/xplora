using Leopotam.EcsLite;
using System;
using System.Collections.Generic;

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
                   EqualityComparer<EcsPackedEntityWithWorld>.Default.Equals(Item1, key.Item1) &&
                   EqualityComparer<EcsPackedEntityWithWorld>.Default.Equals(Item2, key.Item2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2);
        }
    }
}
