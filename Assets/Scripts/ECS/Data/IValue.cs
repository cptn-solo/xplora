﻿using UnityEngine.Playables;

namespace Assets.Scripts.ECS.Data
{
    public interface IValue<T>
    {
        public T Value { get; set; }

        void Add(int b);
        void Combine(int b);
        void Combine(float b);

        public static IValue<T> operator +(IValue<T> a, int b)
        {
            a.Add(b);
            return a;
        }

        public static IValue<T> operator *(IValue<T> a, int b)
        {
            a.Combine(b);
            return a;
        }
        public static IValue<T> operator *(IValue<T> a, float b)
        {
            a.Combine(b);
            return a;
        }
    }

    public interface IIntValue : IValue<int>
    {
    }

    public interface IName : IValue<string>
    {
        public string Name { get; }
    }
}


