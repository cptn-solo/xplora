namespace Assets.Scripts.Data
{
    public struct HeroTraitInfo
    {
        public HeroTrait Trait { get; internal set; }
        public int Level { get; internal set; }

        public static HeroTraitInfo Zero(HeroTrait trait)
        {
            return new HeroTraitInfo() { Trait = trait, Level = 0 };
        }
    }
}