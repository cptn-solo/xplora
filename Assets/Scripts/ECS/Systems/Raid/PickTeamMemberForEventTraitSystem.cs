using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using Random = UnityEngine.Random;
using static UnityEngine.EventSystems.EventTrigger;

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

            if (!PickTheLuckyOne(trait, out var luckyHero, out var level))
                return false;

            var heroConfigRefPool = ecsWorld.Value.GetPool<HeroConfigRefComp>();
            ref var heroConfigRef = ref heroConfigRefPool.Get(luckyHero);

            if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                throw new Exception("No Hero config");

            ref var hero = ref libWorld.GetPool<Hero>().Get(libEntity);
            eventHero = hero;
            eventHeroEntity = ecsWorld.Value.PackEntityWithWorld(luckyHero);
            maxLevel = level;

            return true;
        }

        private bool PickTheLuckyOne(HeroTrait trait, out int luckyHero, out int level)
        {
            luckyHero = -1;
            level = 0;

            switch (trait)
            {
                case HeroTrait.Hidden:
                    return TryGetTeamMemberForTrait<TraitHiddenTag>(out luckyHero, out level);
                case HeroTrait.Purist:
                    return TryGetTeamMemberForTrait<TraitPuristTag>(out luckyHero, out level);
                case HeroTrait.Shrumer:
                    return TryGetTeamMemberForTrait<TraitShrumerTag>(out luckyHero, out level);
                case HeroTrait.Scout:
                    return TryGetTeamMemberForTrait<TraitScoutTag>(out luckyHero, out level);
                case HeroTrait.Tidy:
                    return TryGetTeamMemberForTrait<TraitTidyTag>(out luckyHero, out level);
                case HeroTrait.Soft:
                    return TryGetTeamMemberForTrait<TraitSoftTag>(out luckyHero, out level);
                default:
                    return false;
            }
        }

        protected virtual bool TryGetTeamMemberForTrait<C>(out int luckyOne, out int level)
            where C : struct
        {
            luckyOne = -1;
            level = 0;

            return false;
        }
    }
}