using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    using AssetDict = Dictionary<int, Asset>;

    public struct Team : IEntity
    {
        private int id;
        private string name;
        private AssetDict inventory; // index, item

        public static Team Create(int id, string name)
        {
            Team team = default;
            team.id = id;
            team.name = name;
            team.inventory = DefaultInventory();

            return team;
        }

        public int Id => id;
        public string Name => name;
        public AssetDict Inventory => inventory; // index, item

        #region Assets
        public int GiveAsset(Asset asset, int index = -1) =>
            inventory.PutAsset(asset, index);

        public Asset TakeAsset(AssetType assetType, int count)
        {
            throw new System.NotImplementedException();
        }
        public Asset TakeAsset(int index, int count)
        {
            return inventory.TakeAsset(index);
        }

        private static AssetDict DefaultInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default}, {4, default},
            {5, default}, {6, default}, {7, default}, {8, default}, {9, default},
            {10, default}, {11, default}, {12, default}, {13, default}, {14, default},
        };
        #endregion

    }


}