using Assets.Scripts.UI.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    using AssetDict = Dictionary<int, Asset>;

    public partial struct Hero // Fields
    {
        public int Id { get; private set; }
        public string Name { get; internal set; }
        public HeroDomain Domain { get; internal set; }

        public HeroType HeroType;
        public string IconName;
        public string IdleSpriteName;

        #region Dynamic

        public int TeamId { get; set; }
        public BattleLine Line { get; set; }
        public int Position { get; set; }
        public int HealthCurrent { get; internal set; }
        
        #endregion

        public bool Ranged => AttackType == AttackType.Ranged;

        #region Specs
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

        #endregion

        #region Traits //черты

        public Dictionary<HeroTrait, HeroTraitInfo> Traits { get; internal set; }

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
        private List<BarInfo> barsInfo;
        public List<BarInfo> BarsInfo
        {
            get
            {
                if (barsInfo == null)
                {
                    var list = new List<BarInfo>
                    {
                        BarInfo.EmptyBarInfo(0, $"HP: {Health}", Color.red, Health / Mathf.Max(Health, 100f)),
                        BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", Color.blue, Speed / Mathf.Max(Speed, 10f)),
                        BarInfo.EmptyBarInfo(2, $"Max Damage: {DamageMax}", Color.yellow, DamageMax / Mathf.Max(DamageMax, 20f)),
                        BarInfo.EmptyBarInfo(3, $"Defence: {DefenceRate} %", Color.gray, DefenceRate / Mathf.Max(DefenceRate, 50f)),
                        BarInfo.EmptyBarInfo(4, $"Accuracy: {AccuracyRate}%", Color.cyan, AccuracyRate / Mathf.Max(AccuracyRate, 100f)),
                        BarInfo.EmptyBarInfo(5, $"Dodge: {DodgeRate}%", Color.white, DodgeRate / Mathf.Max(DodgeRate, 50f)),
                        BarInfo.EmptyBarInfo(6, $"Critical Hit: {CriticalHitRate}%", Color.black, CriticalHitRate / Mathf.Max(CriticalHitRate, 10f)),
                    };
                    barsInfo = list;
                }
                return barsInfo;
            }
        }
        public List<BarInfo> BarsInfoShort => BarsInfo.GetRange(0, 2);
        public List<BarInfo> BarsInfoBattle => new() {
            BarInfo.EmptyBarInfo(0, $"HP: {HealthCurrent}", Color.red, (float)HealthCurrent / Health),
            BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", null, Speed / Mathf.Max(Speed, 10f)),
        };

        public Dictionary<DamageEffect, int> ActiveEffects { get; internal set; }

        #endregion

        #region Visual/Inventory
        public AssetDict Inventory { get; private set; } // index, item
        public AssetDict Attack; // index, item
        public AssetDict Defence; // index, item

        #endregion
    }
}