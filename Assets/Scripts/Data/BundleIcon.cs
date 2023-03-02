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
                _ => ""
            };
    }

    public enum BundleIcon
    {
        NA = 0,
        Pause = 100,
        Drop = 200,
        ShieldCrossed = 300,
        Flame = 400,
        SnowFlake = 500,

    }
}