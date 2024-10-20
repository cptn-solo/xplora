﻿using Assets.Scripts.Data;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Assets.Scripts.ECS.Data
{
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


