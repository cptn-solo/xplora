namespace Assets.Scripts.Data
{
    public static class HeroTraitExtensions
    {
        public static HeroTraitInfo Zero(this HeroTrait trait) =>
            new () { Trait = trait, Level = 0 };

        public static HeroTraitInfo Level(this HeroTrait trait, int level) =>
            new() { Trait = trait, Level = level };
    }
}