using System.Collections;

namespace Assets.Scripts.UI.Data
{
    public struct Asset
    {
        public IEntity Owner;
        public int Count;
        public int MaxCount;

        public AssetType AssetType;
        public string IconName;
        public string Name;
        internal string Code;

        public override string ToString() =>
            $"{AssetType} {Name} {IconName}";

        public static Asset EmptyAsset(AssetType assetType, string name, string iconName, int maxCount = 1)
        {
            Asset asset = default;

            asset.AssetType = assetType;
            asset.Name = name;
            asset.IconName = iconName;
            asset.MaxCount = maxCount;
            asset.Code = $"{asset.AssetType} {asset.Name}";

            return asset;
        }        

        public Asset CopyFrom(Asset asset)
        {
            AssetType = asset.AssetType;
            Name = asset.Name;
            IconName = asset.IconName;
            MaxCount = asset.MaxCount;
            Code = $"{asset.AssetType} {asset.Name}";

            return this;

        }

        public Asset GiveAsset(IEntity owner, int count)
        {
            Owner = owner;
            Count = count;

            Owner.GiveAsset(this);

            return this;
        }
    }


}