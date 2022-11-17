using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    public interface IIdentifiable<T>
    {
        public T Id { get; }
    }
    public interface IEntity : IIdentifiable<int>
    {
        public int Id { get; }
        public string Name { get; }
        public Dictionary<int, Asset> Inventory { get; } // index, item

        public int GiveAsset(Asset asset, int index = -1);
        public Asset TakeAsset(AssetType assetType, int count = -1);
        public Asset TakeAsset(int index, int count = -1);
    }


}