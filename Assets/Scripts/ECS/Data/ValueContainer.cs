using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    public struct NameValueComp<T> : IName
    {
        public string Name { get; set; }
        public string Value { get => Name; set => Name = value; }

        public void Add(int b) =>
            throw new System.NotImplementedException();

        public void Combine(int b) =>
            throw new System.NotImplementedException();

        public void Combine(float b) =>
            throw new System.NotImplementedException();
    }

    public struct IntValueComp<T> : IIntValue
    {
        /// <summary>
        /// Value restriction, usefull for scores etc. that can't go negative
        /// </summary>
        public IntRange Boundary { get; set; }

        public int Value { get; set; }

        public void Add(int b)
        {
            if (Boundary == null)
                Value += b;
            else
                Value = Mathf.Clamp(Value + b, Boundary.MinRate, Boundary.MaxRate);
        }

        public void Combine(int b)
        {
            if (Boundary == null)
                Value *= b;
            else
                Value = Mathf.Clamp(Value * b, Boundary.MinRate, Boundary.MaxRate);
   
        }

        public void Combine(float b)
        {
            if (Boundary == null)
                Value = (int)(Value * b);
            else
                Value = Mathf.Clamp((int)(Value * b), Boundary.MinRate, Boundary.MaxRate);
        }
    }

    public struct IntRangeValueComp<T> : IValue<IntRange>
    {
        public IntRange Value { get; set; }

        public int RandomValue => Value.RandomValue;

        public void Add(int b) =>
            Value += b;

        public void Combine(int b) =>
            Value *= b;

        public void Combine(float b) =>
            Value *= b;
    }
}
