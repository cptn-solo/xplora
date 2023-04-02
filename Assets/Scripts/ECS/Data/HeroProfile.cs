using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Assets.Scripts.ECS.Data
{
    public struct NameValueComp<T> : IName
    {
        public string Name { get; set; }
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

    public struct NameTag { }
    public struct IconTag { }
    public struct IdleSpriteTag { }

    public struct DamageRangeTag { }
    public struct CritRateTag { }
    public struct DefenceRateTag { }
    public struct AccuracyRateTag { }
    public struct DodgeRateTag { }
    public struct HealthTag { }
    public struct HpTag { }
    public struct SpeedTag { }

    public struct BuffComp<T> : IIntValue
    {
        public int Value { get; set; }
        public int Usages { get; set; }
        public Color IconColor { get; set; }
        public BundleIcon Icon { get; set; }

        public void Add(int b) =>
            Value += b;

        public void Combine(int b) =>
            Value *= b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

    public struct DummyBuff { }

    public struct TraitHiddenTag { }
    public struct TraitPuristTag { }
    public struct TraitShrumerTag { }
    public struct TraitScoutTag { }
    public struct TraitTidyTag { }
    public struct TraitSoftTag { }
    public struct TraitDummyTag { } // NA


    public struct HeroKindAscTag { }
    public struct HeroKindSpiTag { }
    public struct HeroKindIntTag { }
    public struct HeroKindChaTag { }
    
    public struct HeroKindTemTag { }
    public struct HeroKindConTag { }
    public struct HeroKindStrTag { }
    public struct HeroKindDexTag { }    
    
    public struct KindGroupNeutralTag { }
    public struct KindGroupBodyTag { }
    public struct KindGroupSpiritTag { }

    public struct HeroKindRSDTag { } // stirit kinds ++, body kinds --
  
    /// <summary>
    /// Marks a value of current relation score between some parties
    /// </summary>
    public struct RelationScoreTag { } 

    /// <summary>
    /// Contains references to each score entity by hero instance entity,
    /// so when event is spawned or we just need to check a score with some other guy
    /// just pick a score entity for this guys entity
    /// </summary>
    public struct RelationScoreRef
    {
        public Dictionary<EcsPackedEntity, EcsPackedEntity> Parties { get; set; }
    }

    public struct BarsInfoComp {

        public string Name { get; set; }

        public int Health { get; set; }
        public int Speed { get; set; }
        public int DamageMax { get; set; }
        public int DefenceRate { get; set; }
        public int AccuracyRate { get; set; }
        public int DodgeRate { get; set; }
        public int CriticalHitRate { get; set; }

        private BarInfo[] barsInfo;

        public void Generate()
        {
            barsInfo = new BarInfo[]
        {
            BarInfo.EmptyBarInfo(0, $"Health: {Health}", Color.red, Health / Mathf.Max(Health, 100f)),
            BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", Color.blue, Speed / Mathf.Max(Speed, 10f)),
            BarInfo.EmptyBarInfo(2, $"Max Damage: {DamageMax}", Color.yellow, DamageMax / Mathf.Max(DamageMax, 20f)),
            BarInfo.EmptyBarInfo(3, $"Defence: {DefenceRate} %", Color.gray, DefenceRate / Mathf.Max(DefenceRate, 50f)),
            BarInfo.EmptyBarInfo(4, $"Accuracy: {AccuracyRate}%", Color.cyan, AccuracyRate / Mathf.Max(AccuracyRate, 100f)),
            BarInfo.EmptyBarInfo(5, $"Dodge: {DodgeRate}%", Color.white, DodgeRate / Mathf.Max(DodgeRate, 50f)),
            BarInfo.EmptyBarInfo(6, $"Critical Hit: {CriticalHitRate}%", Color.black, CriticalHitRate / Mathf.Max(CriticalHitRate, 10f)),
        };
        }
        public BarInfo[] BarsInfo => barsInfo;

}

}


