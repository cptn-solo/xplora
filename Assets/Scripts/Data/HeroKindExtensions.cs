namespace Assets.Scripts.Data
{
    public static class HeroKindExtensions
    {
        public static HeroKindInfo Zero(this HeroKind kind) =>
            new () { Kind = kind, Level = 0 };

        public static HeroKindInfo Level(this HeroKind kind, int level) =>
            new() { Kind = kind, Level = level };

        public static string Name(this HeroKind kind) =>
            kind switch
            {
                HeroKind.Asc => "Аскетизм",
                HeroKind.Spi => "Духовность",
                HeroKind.Int => "Умствование",
                HeroKind.Cha => "Ментальное влияние",
                HeroKind.Tem => "Физический соблазн",
                HeroKind.Con => "Телесность",
                HeroKind.Str => "Решительность",
                HeroKind.Dex => "Физическая хитрость",
                _ => "Характеристика героя"
                };

        public static string ShortName(this HeroKind kind) =>
            kind switch
            {
                HeroKind.Asc => "Аскетизм",
                HeroKind.Spi => "Духовн-ть",
                HeroKind.Int => "Умств-e",
                HeroKind.Cha => "Мент. вл-е",
                HeroKind.Tem => "Физ. сoбл-н",
                HeroKind.Con => "Телесность",
                HeroKind.Str => "Решит-ть",
                HeroKind.Dex => "Физ. хитр-ть",
                _ => "Характеристика героя"
            };
    }
}