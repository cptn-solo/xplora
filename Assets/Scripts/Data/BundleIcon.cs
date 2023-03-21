namespace Assets.Scripts.Data
{
    public static class BundleIconExtensions
    {
        public static string IconFileName(this BundleIcon iconCode) =>
            iconCode switch
            {
                BundleIcon.Pause => "Effects/stunned",
                BundleIcon.Drop => "Effects/bleeding",
                BundleIcon.ShieldCrossed => "Effects/pierced",
                BundleIcon.Flame => "Effects/burning",
                BundleIcon.SnowFlake => "Effects/frozing",
                BundleIcon.Power => "Icons/Assets/power",
                BundleIcon.Coins => "Icons/Assets/gold_icon",
                BundleIcon.Sword => "Icons/Assets/improved_damage_icon",
                _ => ""
            };

        public static BundleIconMaterial IconMaterial(this BundleIcon iconCode) =>
            iconCode switch
            {
                BundleIcon.Coins => BundleIconMaterial.NA,
                BundleIcon.Sword => BundleIconMaterial.NA,
                _ => BundleIconMaterial.Font
            };
    }

    public enum BundleIconMaterial
    {
        NA = 0,
        Font = 100,
        Custom = 200,
    }

    public enum BundleIcon
    {
        NA = 0,
        Pause = 100,
        Drop = 200,
        ShieldCrossed = 300,
        Flame = 400,
        SnowFlake = 500,
        Power = 600,
        Coins = 700,
        Sword = 800,

    }
}