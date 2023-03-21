namespace Assets.Scripts.Data
{
    public enum AssetType
    {
        NA = 0,
        Attack = 100,
        Defence = 200,
        Resource = 300,
        Money = 300,
    }

    public static class AssetTypeExtensions
    {
        public static BundleIcon Icon(this AssetType assetType) =>
            assetType switch
            {
                AssetType.Money => BundleIcon.Coins,
                _ => BundleIcon.NA
            };
    }

}