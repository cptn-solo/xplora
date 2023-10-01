using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public static class EcsHeroKindsExtensions
    {
        public static HeroKindBarInfo GetHeroKindBarInfo(this EcsWorld world, int entity, HeroKind kind, int diffValue = 0)
        {
            var kindValue = GetHeroKindValue(world, entity, kind);
            HeroKindBarInfo kindInfo = new()
            {
                BarInfo = new BarInfo
                {
                    Title = $"{kindValue}",
                    Value = kindValue,
                    Delta = diffValue,
                    Color = Color.red,
                    DeltaColor = diffValue > 0 ? Color.cyan : Color.yellow,
                },
                CurrentValue = kindValue,
                Kind = kind,
                ItemTitle = kind.Name(),                
            };
            return kindInfo;
        }

        public static int GetHeroKindValue(this EcsWorld world, int entity, HeroKind kind)
        {
            return kind switch
            {
                HeroKind.Asc => world.ReadIntValue<HeroKindAscTag>(entity),
                HeroKind.Spi => world.ReadIntValue<HeroKindSpiTag>(entity),
                HeroKind.Int => world.ReadIntValue<HeroKindIntTag>(entity),
                HeroKind.Cha => world.ReadIntValue<HeroKindChaTag>(entity),
                HeroKind.Tem => world.ReadIntValue<HeroKindTemTag>(entity),
                HeroKind.Con => world.ReadIntValue<HeroKindConTag>(entity),
                HeroKind.Str => world.ReadIntValue<HeroKindStrTag>(entity),
                HeroKind.Dex => world.ReadIntValue<HeroKindDexTag>(entity),
                _ => 0,
            };

        }


    }
}
