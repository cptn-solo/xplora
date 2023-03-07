using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitTerrainAttributeSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<VisitedComp<TerrainAttributeComp>> pool;
        private readonly EcsFilterInject<Inc<VisitedComp<TerrainAttributeComp>>> visitFilter;

        private readonly EcsCustomInject<RaidService> raidService;
        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var attributes = ref pool.Value.Get(entity);
                ProcessTerrainAttribute(attributes.Info.TerrainAttribute);
            }
        }

        private void ProcessTerrainAttribute(TerrainAttribute attribute)
        {
            if (raidService.Value.Dialog == null)
                return;

            //NB: for now it is 1:1 mapping, can be extended later
            var eventConfig = worldService.Value.TerrainEventsLibrary.TerrainEvents[attribute];

            if (!GetActiveTeamMemberForTrait(eventConfig.Trait,
                out var hero, out var heroEntity, out var maxLevel))
                return;

            raidService.Value.TryCastEcsTerrainEvent(eventConfig,
                hero.Value, heroEntity.Value, maxLevel);
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