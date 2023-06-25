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
                // bulk uploaded new icons from the Звуки/Иконки list:
                BundleIcon.Accuracy => "Icons/accuracy",
                BundleIcon.ArmorPenetr => "Icons/armor_penetr",
                BundleIcon.Avoid => "Icons/avoid",
                BundleIcon.Bleeding => "Icons/bleeding",
                BundleIcon.Burning => "Icons/burning",
                BundleIcon.ColdDmg => "Icons/cold_dmg",
                BundleIcon.CritDmg => "Icons/crit_dmg",
                BundleIcon.CuttingDmg => "Icons/cutting_dmg",
                BundleIcon.DamageBonuses => "Icons/damage_bonuses",
                BundleIcon.Defence => "Icons/defence",
                BundleIcon.FireDmg => "Icons/fire_dmg",
                BundleIcon.Freezing => "Icons/freezing",
                BundleIcon.GoldIcon => "Icons/gold_icon",
                BundleIcon.ImprovedDamageIcon => "Icons/improved_damage_icon",
                BundleIcon.MarkTarget => "Icons/mark_target",
                BundleIcon.MaxDmg => "Icons/max_dmg",
                BundleIcon.MinDmg => "Icons/min_dmg",
                BundleIcon.PierceDmg => "Icons/pierce_dmg",
                BundleIcon.PowerDmg => "Icons/power_dmg",
                BundleIcon.Revenge => "Icons/revenge",
                BundleIcon.Stun => "Icons/stun",
                BundleIcon.AimTarget => "Icons/enemy_target",

                _ => ""
            };

        public static BundleIconMaterial IconMaterial(this BundleIcon iconCode) =>
            iconCode switch
            {
                BundleIcon.Coins => BundleIconMaterial.NA,
                BundleIcon.GoldIcon => BundleIconMaterial.NA,
                BundleIcon.Sword => BundleIconMaterial.NA,
                BundleIcon.ImprovedDamageIcon => BundleIconMaterial.NA,
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
        Pause = 100, //Stun
        Drop = 200, //Bleeding
        ShieldCrossed = 300, //ArmorPenetr
        Flame = 400, //Burning
        SnowFlake = 500, //Freezing
        Power = 600, //CritDmg
        Coins = 700, //GoldIcon
        Sword = 800, //ImprovedDamageIcon

        Accuracy = 1000,
        ArmorPenetr = 1010,
        Avoid = 1020,
        Bleeding = 1030,
        Burning = 1040,
        ColdDmg = 1050,
        CritDmg = 1060,
        CuttingDmg = 1070,
        DamageBonuses = 1080,
        Defence = 1090,
        FireDmg = 1100,
        Freezing = 1110,
        GoldIcon = 1120,
        ImprovedDamageIcon = 1130,
        MarkTarget = 1140,
        MaxDmg = 1150,
        MinDmg = 1160,
        PierceDmg = 1170,
        PowerDmg = 1180,
        Revenge = 1190,
        Stun = 1200,

        AimTarget = 1210, // target highliter for revenge/target battle relation effects
    }
}