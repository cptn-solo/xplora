using System;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Data
{
    public class ValueRange<T> : Tuple<T, T>
    {
        public ValueRange(T item1, T item2) : base(item1, item2)
        {
        }

        public T MinRate => Item1;
        public T MaxRate => Item2;
 
        public virtual bool Contains(T a) => false;

        public override bool Equals(object obj)
        {
            return obj is ValueRange<T> target &&
                target.MinRate.Equals(MinRate) &&
                target.MaxRate.Equals(MaxRate);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MinRate, MaxRate);
        }

        public override string ToString()
        {
            return $"{MinRate}-{MaxRate}";
        }
    }

    public class FloatRange : ValueRange<float>
    {
        public FloatRange(float item1, float item2) : base(item1, item2)
        {
        }

        public float RandomValue => Random.Range(MinRate, MaxRate);

        public override bool Contains(float a) => 
            MaxRate >= a && MinRate <= a;
    }

    public class IntRange : ValueRange<int>
    {
        public IntRange(int item1, int item2) : base(item1, item2)
        {
        }

        public int RandomValue => Random.Range(MinRate, MaxRate + 1);

        public override bool Contains(int a) => 
            MaxRate >= a && MinRate <= a;

        public static IntRange operator +(IntRange a, IntRange b)
            => new(a.MinRate + b.MinRate, a.MaxRate + b.MaxRate);

        public static IntRange operator +(IntRange a, int b)
            => new(a.MinRate + b, a.MaxRate + b);

        public static IntRange operator *(IntRange a, IntRange b)
            => new(a.MinRate * b.MinRate, a.MaxRate * b.MaxRate);

        public static IntRange operator *(IntRange a, int b)
            => new(a.MinRate * b, a.MaxRate * b);

        public static IntRange operator *(IntRange a, float b)
            => new((int)(a.MinRate * b), (int)(a.MaxRate * b));
    }
}