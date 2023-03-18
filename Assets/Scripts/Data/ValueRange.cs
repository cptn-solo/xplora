using System;
using Google.Apis.Sheets.v4.Data;

namespace Assets.Scripts.Data
{
    public class ValueRange<T> : Tuple<T, T>
    {
        public ValueRange(T item1, T item2) : base(item1, item2)
        {
        }

        public T MinRate => Item1;
        public T MaxRate => Item2;

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
    }

    public class IntRange : ValueRange<int>
    {
        public IntRange(int item1, int item2) : base(item1, item2)
        {
        }
    }
}