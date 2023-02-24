using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    using AssetDict = Dictionary<int, Asset>;
    public partial struct Hero : IEntity
    {
        public override string ToString()
        { 
            return $"#{Id} {Name}";
        }
        public override bool Equals(object obj)
        {
            if (obj is Hero hero)
                return Id == hero.Id && HeroType == hero.HeroType;
            else return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, HeroType);
        }

        public static Hero Default =>
            EmptyHero();

        public static Hero EmptyHero() =>
            EmptyHero(-1, "", null, null, HeroType.NA);

        public static Hero EmptyHero(int id, string name,
            string iconName = "hero1", 
            string idleSpriteName = "hero2",
            HeroType heroType = HeroType.Human)
        {
            Hero hero = default;
            hero.Name = name;
            hero.Id = id;
            hero.Inventory = DefaultInventory();
            hero.HeroType = heroType;
            hero.IconName = iconName;
            hero.IdleSpriteName = idleSpriteName;

            hero.Traits = DefaultTraits();

            hero.Attack = DefaultAttack();
            hero.Defence = DefaultDefence();

            return hero;
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

        public static AssetDict DefaultDefence() => new() {
            {0, default}, {1, default},
        };

        public static AssetDict DefaultAttack() => new() {
            {0, default}, {1, default},
        };

        public static AssetDict DefaultInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default}, {4, default},
            {5, default}, {6, default}, {7, default}, {8, default}, {9, default},
        };

        public static Dictionary<HeroTrait, HeroTraitInfo> DefaultTraits() => new()
            {
                { HeroTrait.Hidden, HeroTrait.Hidden.Zero() },
                { HeroTrait.Purist, HeroTrait.Purist.Zero()  },
                { HeroTrait.Shrumer, HeroTrait.Shrumer.Zero()  },
                { HeroTrait.Scout, HeroTrait.Scout.Zero()  },
                { HeroTrait.Tidy, HeroTrait.Tidy.Zero()  },
                { HeroTrait.Soft, HeroTrait.Soft.Zero() },
            };
    }

}