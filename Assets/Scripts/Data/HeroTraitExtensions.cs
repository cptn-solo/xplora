using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

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

        public static string BonusOptionName(this HeroTrait trait, int factor) =>
            trait switch
            {
                HeroTrait.Hidden => $"{trait.Name()} +{factor}",
                HeroTrait.Purist => $"{trait.Name()} +{factor}",
                HeroTrait.Shrumer => $"{trait.Name()} +{factor}",
                HeroTrait.Scout => $"{trait.Name()} +{factor}",
                HeroTrait.Tidy => $"{trait.Name()} +{factor}",
                HeroTrait.Soft => $"{trait.Name()} +{factor}",
                _ => $"{trait.Name()} +{factor}"
            };
    }
}