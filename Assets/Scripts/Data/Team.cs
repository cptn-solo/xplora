using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    using AssetDict = Dictionary<int, Asset>;

    public struct Team : IEntity
    {
        public Asset[] Assets { get; set; } // keeps balances

        public override string ToString()
        {
            return $"Команда {Id}: {Name}" ;
        }

        public static Team Create(int id, string name)
        {
            Team team = default;
            team.Id = id;
            team.Name = name;
            team.Inventory = DefaultInventory();

            return team;
        }

        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public AssetDict Inventory { get; internal set; } // legacy, remove

        #region Assets
        public int GiveAsset(Asset asset, int index = -1) =>
            Inventory.PutAsset(asset, index);

        public Asset TakeAsset(AssetType assetType, int count)
        {
            throw new System.NotImplementedException();
        }
        public Asset TakeAsset(int index, int count)
        {
            return Inventory.TakeAsset(index);
        }

        public Team ResetAssets()        
        {
            Inventory = DefaultInventory();
            return this;
        }

        public static AssetDict DefaultInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default}, {4, default},
            {5, default}, {6, default}, {7, default}, {8, default}, {9, default},
            {10, default}, {11, default}, {12, default}, {13, default}, {14, default},
        };

        #endregion

    }


}