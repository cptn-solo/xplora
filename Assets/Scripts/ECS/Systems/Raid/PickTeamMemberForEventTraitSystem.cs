using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    public class PickTeamMemberForEventTraitSystem<T> : IEcsRunSystem
        where T : struct
    {
        protected readonly EcsWorldInject ecsWorld;

        protected readonly EcsPoolInject<VisitedComp<T>> pool;
        protected readonly EcsPoolInject<ActiveTraitHeroComp<T>> activeTraitHeroPool;

        protected readonly EcsFilterInject<Inc<VisitedComp<T>>> visitFilter;

        protected readonly EcsCustomInject<RaidService> raidService;
        protected readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                var trait = TraitForEvent(entity);

                if (!PickHeroForTrait(trait, entity) &&
                    activeTraitHeroPool.Value.Has(entity))
                    activeTraitHeroPool.Value.Del(entity);
            }
        }
        protected virtual HeroTrait TraitForEvent(int entity)
        {
            return HeroTrait.NA;
        }

        private bool PickHeroForTrait(HeroTrait trait, int entity)
        {
            if (!GetActiveTeamMemberForTrait(trait,
                out var hero, out var packed, out var maxLevel))
                return false;

            if (!activeTraitHeroPool.Value.Has(entity))
                activeTraitHeroPool.Value.Add(entity);

            ref var activeTraitHero = ref activeTraitHeroPool.Value.Get(entity);
            activeTraitHero.Trait = trait;
            activeTraitHero.PackedHeroInstanceEntity = packed.Value;
            activeTraitHero.Hero = hero.Value;
            activeTraitHero.MaxLevel = maxLevel;

            return true;
        }

        private bool GetActiveTeamMemberForTrait(HeroTrait trait,
            out Hero? eventHero,
            out EcsPackedEntityWithWorld? eventHeroEntity,
            out int maxLevel)
        {

            eventHero = null;
            eventHeroEntity = null;
            maxLevel = 0;

            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return false;

            var filter = ecsWorld.Value.Filter<PlayerTeamTag>().Inc<HeroConfigRefComp>().End();
            var heroConfigRefPool = ecsWorld.Value.GetPool<HeroConfigRefComp>();

            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();
            var maxedHeroes = ListPool<Hero>.Get();

            foreach (var entity in filter)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Get(entity);
                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero config");

                ref var hero = ref libWorld.GetPool<Hero>().Get(libEntity);

                if (hero.Traits.TryGetValue(trait, out var traitInfo) &&
                    traitInfo.Level > 0 &&
                    traitInfo.Level >= maxLevel)
                {
                    maxLevel = traitInfo.Level;
                    maxedHeroes.Add(hero);
                    buffer.Add(ecsWorld.Value.PackEntityWithWorld(entity));
                }
            }
            if (maxedHeroes.Count > 0)
            {
                var idx = Random.Range(0, maxedHeroes.Count);
                eventHero = maxedHeroes[idx];
                eventHeroEntity = buffer[idx];
            }

            ListPool<Hero>.Add(maxedHeroes);

            var retval = buffer.Count > 0;

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;
        }
    }
}