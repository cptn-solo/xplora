namespace Assets.Scripts.Data
{
    public enum RelationsEffectType
    {
        NA = 0,
        SpecMaxMin = 100,
        SpecAbs = 200,
        SpecPercent = 300,
        DmgEffectAbs = 400,
        DmgEffectPercent = 500,
        DmgEffectBonusAbs = 600,
        DmgEffectBonusPercent = 700,
        DmgEffectBonusKey = 750, // to use as a key as no matter wich kind of the effect bonus has a party at the moment of cast
        AlgoRevenge = 800,
        AlgoTarget = 900,
        AlgoDamageTypeBlock = 1000,
    }
}