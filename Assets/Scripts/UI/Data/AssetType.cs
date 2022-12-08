namespace Assets.Scripts.UI.Data
{
    public enum AssetType
    {
        NA = 0,
        Attack = 100,
        Defence = 200,
        Resource = 300,
    }

    public enum HeroType
    {
        NA = 0,
        Human = 100,
    }

    public enum AttackType
    {
        NA = 0,
        Melee = 100,
        Ranged = 200,
        Magic = 300,
    }

    public enum DamageType
    {
        NA = 0,
        Force = 100,    // Силовой
        Cut = 200,      // Режущий
        Pierce = 300,   // Колющий
        Burn = 400,     // Огненный
        Frost = 500,    // Замораживающий
    }

}