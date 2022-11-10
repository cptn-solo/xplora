using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    public struct Hero : IEntity
    {
        private int id;
        private string name;
        private Dictionary<int, Asset> inventory; // index, item

        public int Id => id;

        public string Name => name;

        public Dictionary<int, Asset> Inventory => inventory; // index, item
        public Dictionary<int, Asset> Attack; // index, item
        public Dictionary<int, Asset> Defence; // index, item

        public static Hero EmptyHero(int id, string name)
        {
            Hero hero = default;
            hero.name = name;
            hero.id = id;
            hero.inventory = DefaultHeroInventory();

            hero.Attack = DefaultHeroAttack();
            hero.Defence = DefaultHeroDefence();

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

        private static Dictionary<int, Asset> DefaultHeroDefence() => new() {
            {0, default},{1, default},
        };

        private static Dictionary<int, Asset> DefaultHeroAttack() => new() {
            {0, default},{1, default},
        };

        private static Dictionary<int, Asset> DefaultHeroInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default},{4, default},
            {5, default}, {6, default},{7, default},{8, default},{9, default},
        };

    }

}