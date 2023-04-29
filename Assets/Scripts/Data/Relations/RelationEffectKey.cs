using System;

namespace Assets.Scripts.Data
{
    public class RelationEffectKey : Tuple<SpecOption, DamageEffect, DamageType, RelationsEffectType>
    {
        public SpecOption SpecOption => Item1;
        public DamageEffect DamageEffect => Item2;
        public DamageType DamageType => Item3;
        public RelationsEffectType RelationsEffectType => Item4;

        public RelationEffectKey(SpecOption item1, DamageEffect item2, DamageType item3, RelationsEffectType item4) : 
            base(item1, item2, item3, item4)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as RelationEffectKey;
            return other.Item1 == Item1 && other.Item2 == Item2 && other.Item3 == Item3 && other.Item4 == Item4;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item1, Item2, Item3, Item4);
        }

        public override string ToString()
        {
            return $"{Item1}-{Item2}-{Item3}-{Item4}";
        }
    }
}
