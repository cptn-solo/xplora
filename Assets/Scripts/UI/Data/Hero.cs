using System.Collections.Generic;

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

        public int Id => id;

        public string Name => name;

        public AssetDict Inventory => inventory; // index, item
        public AssetDict Attack; // index, item
        public AssetDict Defence; // index, item

        public static Hero EmptyHero(int id, string name,
            string iconName = "hero1", HeroType heroType = HeroType.Human)
        {
            Hero hero = default;
            hero.name = name;
            hero.id = id;
            hero.inventory = DefaultInventory();
            hero.HeroType = heroType;
            hero.IconName = iconName;

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