using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    public struct HeroProfile
    {
        // placeholder
    }

    public struct NameComp : IName
    {
        public string Name { get; set; }
    }

    public struct IconName : IName
    {
        public string Name { get; set; }
    }

    public struct IdleSpriteName : IName
    {
        public string Name { get; set; }
    }

    public struct DamageRangeComp : IValue<IntRange>
    {
        public int RandomDamage => Random.Range(Value.MinRate, Value.MaxRate + 1);

        public IntRange Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = Value * b;
    }

    public struct CritRateComp : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct DefenceRateComp : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct AccuracyRateComp : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct DodgeRateComp : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct HealthComp : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct SpeedComp : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct BuffComp<T> : IIntValue
    {
        public int Value { get; set; }
        public int Usages { get; set; }
        public Color IconColor { get; set; }
        public BundleIcon Icon { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct DummyBuff : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct HeroTraitComp<T> : IIntValue
    {
        public int Value { get; set; }

        public void Combine(int b) =>
            Value = Value * b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct TraitHiddenTag { }
    public struct TraitPuristTag { }
    public struct TraitShrumerTag { }
    public struct TraitScoutTag { }
    public struct TraitTidyTag { }
    public struct TraitSoftTag { }
    public struct TraitDummyTag { } // NA

}


