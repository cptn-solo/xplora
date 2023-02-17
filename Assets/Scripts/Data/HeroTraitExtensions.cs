namespace Assets.Scripts.Data
{
    public static class HeroTraitExtensions
    {
        public static HeroTraitInfo Zero(this HeroTrait trait) =>
            new () { Trait = trait, Level = 0 };

        public static HeroTraitInfo Level(this HeroTrait trait, int level) =>
            new() { Trait = trait, Level = level };

        public static string Name(this HeroTrait trait) =>
            trait switch
            {
                HeroTrait.Hidden =>     "Скрытный",
                HeroTrait.Purist =>     "Эстет",
                HeroTrait.Shrumer =>    "Грибник",
                HeroTrait.Scout =>      "Разведчик",
                HeroTrait.Tidy =>       "Чистюля",
                HeroTrait.Soft =>       "Изнеженный",
                _ => "Черта героя"
            };
    }
}