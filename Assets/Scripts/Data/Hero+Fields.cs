using Assets.Scripts.UI.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    using AssetDict = Dictionary<int, Asset>;

    public partial struct Hero // Fields
    {
        public int Id { get; set; }
        public string Name { get; internal set; }
        public HeroDomain Domain { get; internal set; }

        public HeroType HeroType { get; set; }
        public string IconName { get; set; }
        public string IdleSpriteName { get; set; }

        public bool Ranged => AttackType == AttackType.Ranged;

        #region Specs
        //TODO: convert to DamageRange to match SpecOption enum
        public int DamageMin { get; internal set; }
        public int DamageMax { get; internal set; }
        public int DefenceRate { get; internal set; }
        public int AccuracyRate { get; internal set; }
        public int DodgeRate { get; internal set; }
        public int Health { get; internal set; }
        public int Speed { get; internal set; }
        public int CriticalHitRate { get; internal set; }
        public AttackType AttackType { get; internal set; }
        public DamageType DamageType { get; internal set; }
        public int ResistBleedRate { get; internal set; }
        public int ResistPoisonRate { get; internal set; }
        public int ResistStunRate { get; internal set; }
        public int ResistBurnRate { get; internal set; }
        public int ResistFrostRate { get; internal set; }
        public int ResistFlushRate { get; internal set; }

        // for enemy spawner
        public int OveralStrength { get; internal set; }

        #endregion

        #region Traits //черты

        public Dictionary<HeroTrait, HeroTraitInfo> Traits { get; internal set; }

        #endregion

        #region Relations

        public Dictionary<HeroKind, HeroKindInfo> Kinds { get; internal set; }

        #endregion

        #region Sounds
        public string SndAttack { get; internal set; }
        public string SndDodged { get; internal set; }
        public string SndHit { get; internal set; }
        public string SndStunned { get; internal set; }
        public string SndBleeding { get; internal set; }
        public string SndPierced { get; internal set; }
        public string SndBurning { get; internal set; }
        public string SndFreezed { get; internal set; }
        public string SndCritHit { get; internal set; }
        public string SndDied { get; internal set; }

        #endregion

        #region Bars Info
        private BarInfo[] barsInfo;
        private BarInfo[] barsInfoShort;

        public BarInfo[] BarsInfo
        {
            get
            {
                if (barsInfo == null)
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
                    barsInfoShort = new BarInfo[]
                    {
                        BarInfo.EmptyBarInfo(0, $"HP: {Health}", Color.red, Health / Mathf.Max(Health, 100f)),
                        BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", Color.blue, Speed / Mathf.Max(Speed, 10f)),
                    };
                }
                return barsInfo;
            }
        }
        public BarInfo[] BarsInfoShort {
            get {
                if (BarsInfo.Length > 0)
                    return barsInfoShort;
                else return new BarInfo[0];
            }
        }
        public void ResetBarsInfo()
        {
            barsInfo = null;
            barsInfoShort = null;
        }

        #endregion

        #region Visual/Inventory
        public AssetDict Inventory { get; set; } // index, item
        public AssetDict Attack { get; set; }  // index, item
        public AssetDict Defence { get; set; } // index, item

    #endregion
}
}