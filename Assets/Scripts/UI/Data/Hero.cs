using Assets.Scripts.UI.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    using AssetDict = Dictionary<int, Asset>;
    public struct Hero : IEntity
    {
        private int id;
        private string name;
        private AssetDict inventory; // index, item

        public HeroType HeroType;
        public string IconName;

        // specs
        private int damageMin;  // Урон
        private int damageMax;

        private int defenceRate;    // Защита, %
        private int accuracyRate;   // Точность, %
        private int dodgeRate;      // Уклонение, %
        private int health;     // Здоровье
        private int speed;      // Скорость
        private int criticalHitRate;// Критический удар, %
        
        private AttackType attackType; // Тип атаки
        private DamageType damageType; // Тип урона

        private int resistBleedRate;    // Сопротивляемость кровотечению, %
        private int resistPoisonRate;   // Сопротивляемость яду, %
        private int resistStunRate;     // Сопротивляемость оглушению, %
        private int resistBurnRate;     // Сопротивляемость горению, %
        private int resistFrostRate;    // Сопротивляемость холоду, %
        private int resistFlushRate;    // Споротивляемость ослеплению, %
        // end specs

        public int TeamId { get; set; }

        public int Id => id;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public AssetDict Inventory => inventory; // index, item
        public AssetDict Attack; // index, item
        public AssetDict Defence; // index, item

        public static Hero Default => EmptyHero();

        public int DamageMin { get => damageMin; set => damageMin = value; }
        public int DamageMax { get => damageMax; set => damageMax = value; }
        public int DefenceRate { get => defenceRate; set => defenceRate = value; }
        public int AccuracyRate { get => accuracyRate; set => accuracyRate = value; }
        public int DodgeRate { get => dodgeRate; set => dodgeRate = value; }
        public int Health { get => health; set => health = value; }
        public int Speed { get => speed; set => speed = value; }
        public int CriticalHitRate { get => criticalHitRate; set => criticalHitRate = value; }
        public AttackType AttackType { get => attackType; set => attackType = value; }
        public DamageType DamageType { get => damageType; set => damageType = value; }
        public int ResistBleedRate { get => resistBleedRate; set => resistBleedRate = value; }
        public int ResistPoisonRate { get => resistPoisonRate; set => resistPoisonRate = value; }
        public int ResistStunRate { get => resistStunRate; set => resistStunRate = value; }
        public int ResistBurnRate { get => resistBurnRate; set => resistBurnRate = value; }
        public int ResistFrostRate { get => resistFrostRate; set => resistFrostRate = value; }
        public int ResistFlushRate { get => resistFlushRate; set => resistFlushRate = value; }

        private List<BarInfo> barsInfo;
        public List<BarInfo> BarsInfo { 
            get
            {
                if (barsInfo == null)
                {
                    var list = new List<BarInfo>
                    {
                        BarInfo.EmptyBarInfo(0, $"HP: {Health}", Color.red, Health / 100f),
                        BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", Color.blue, Speed / 10f),
                        BarInfo.EmptyBarInfo(2, $"Max Damage: {DamageMax}", Color.yellow, DamageMax / 20f),
                        BarInfo.EmptyBarInfo(3, $"Defence: {DefenceRate} %", Color.gray, DefenceRate / 50f),
                        BarInfo.EmptyBarInfo(4, $"Accuracy: {AccuracyRate}%", Color.cyan, AccuracyRate / 100f),
                        BarInfo.EmptyBarInfo(5, $"Dodge: {DodgeRate}%", Color.white, DodgeRate / 50f),
                        BarInfo.EmptyBarInfo(6, $"Critical Hit: {CriticalHitRate}%", Color.black, CriticalHitRate / 10f),
                    };
                    barsInfo = list;
                }
                return barsInfo;
            } 
        }
        public List<BarInfo> BarsInfoShort => BarsInfo.GetRange(0, 2);

        public static Hero EmptyHero() => 
            EmptyHero(-1, "", null, HeroType.NA);

        public static Hero EmptyHero(int id, string name,
            string iconName = "hero1", HeroType heroType = HeroType.Human)
        {
            Hero hero = default;
            hero.name = name;
            hero.id = id;
            hero.inventory = DefaultInventory();
            hero.HeroType = heroType;
            hero.IconName = iconName;
            hero.TeamId = -1;

            hero.Attack = DefaultAttack();
            hero.Defence = DefaultDefence();

            return hero;
        }

        public int GiveAsset(Asset asset, int index = -1) =>
            inventory.PutAsset(asset, index);

        public Asset TakeAsset(AssetType assetType, int count)
        {
            throw new System.NotImplementedException();
        }
        public Asset TakeAsset(int index, int count)
        {
            throw new System.NotImplementedException();
        }

        private static AssetDict DefaultDefence() => new() {
            {0, default}, {1, default},
        };

        private static AssetDict DefaultAttack() => new() {
            {0, default}, {1, default},
        };

        private static AssetDict DefaultInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default}, {4, default},
            {5, default}, {6, default}, {7, default}, {8, default}, {9, default},
        };

    }

}