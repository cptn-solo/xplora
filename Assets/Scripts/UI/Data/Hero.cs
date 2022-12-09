using Assets.Scripts.UI.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    using AssetDict = Dictionary<int, Asset>;
    public partial struct Hero : IEntity
    {
        public override string ToString()
        {
            return $"#{Id} {Name} (команда {TeamId}, линия {Line}) HP: {HealthCurrent}";
        }

        public static Hero Default =>
            EmptyHero();

        public static Hero EmptyHero() =>
            EmptyHero(-1, "", null, HeroType.NA);

        public static Hero EmptyHero(int id, string name,
            string iconName = "hero1", HeroType heroType = HeroType.Human)
        {
            Hero hero = default;
            hero.Name = name;
            hero.Id = id;
            hero.Inventory = DefaultInventory();
            hero.HeroType = heroType;
            hero.IconName = iconName;
            hero.TeamId = -1;
            hero.Line = BattleLine.NA;

            hero.Attack = DefaultAttack();
            hero.Defence = DefaultDefence();

            return hero;
        }

        public Hero UpdateHealthCurrent(int val)
        {
            HealthCurrent = val;
            return this;
        }

        public int GiveAsset(Asset asset, int index = -1) =>
            Inventory.PutAsset(asset, index);

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