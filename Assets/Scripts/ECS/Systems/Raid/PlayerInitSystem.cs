using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<RaidComp> raidPool = default;
        private readonly EcsPoolInject<PlayerComp> playerPool = default;
        private readonly EcsPoolInject<TeamComp> teamPool = default;
        private readonly EcsPoolInject<HeroComp> heroPool = default;
        private readonly EcsPoolInject<PowerComp> powerPool = default;
        private readonly EcsPoolInject<StaminaComp> staminaPool = default;
        private readonly EcsPoolInject<SightRangeComp> sightRangePool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;

        private readonly EcsCustomInject<RaidService> raidService = default;
        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public void Init(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            ref var raidComp = ref raidPool.Value.Get(raidEntity);

            if (raidComp.PlayerHeroConfigRefs.Length <= 0)
                return;

            var entity = ecsWorld.Value.NewEntity();

            ref var playerComp = ref playerPool.Value.Add(entity);

            ref var teamComp = ref teamPool.Value.Add(entity);

            var heroBuffer = ListPool<Hero>.Get();

            heroBuffer.AddRange(raidComp.PlayerHeroConfigRefs.Select((x) => {                
                var configPacked = libraryService.Value.GetHeroConfigPackedForRefPacked(x, out var heroConfig);                                              
                return heroConfig;
            }));
            var bestSpeed = heroBuffer.ToArray().HeroBestBySpeed(out var idx);

            ref var heroComp = ref heroPool.Value.Add(entity);
            heroComp.Hero = raidComp.PlayerHeroConfigRefs[idx];

            ref var sightRangeComp = ref sightRangePool.Value.Add(entity);
            sightRangeComp.Range = 2;

            ref var staminaComp = ref staminaPool.Value.Add(entity);

            var initialPower = 0;
            foreach (var hero in heroBuffer)
                initialPower += 5 * hero.Health * hero.Speed / heroBuffer.Count;

            ref var powerComp = ref powerPool.Value.Add(entity);
            powerComp.InitialValue = initialPower;
            powerComp.CurrentValue = initialPower;

            // create player team member entitites to track bonuses and such
            for (int i = 0; i < raidComp.PlayerHeroConfigRefs.Length; i++)
            {
                var configRefPacked = raidComp.PlayerHeroConfigRefs[i];
                var configPacked = libraryService.Value.GetHeroConfigPackedForRefPacked(configRefPacked, out _);
                
                var playerTeamMemberEntity = ecsWorld.Value.NewEntity();
                playerTeamTagPool.Value.Add(playerTeamMemberEntity);

                ref var heroConfigRef = ref heroConfigRefPool.Value.Add(playerTeamMemberEntity);
                heroConfigRef.HeroConfigPackedEntity = configPacked;
                heroConfigRef.HeroConfigRefPackedEntity = configRefPacked;
            }

            ListPool<Hero>.Add(heroBuffer);

            raidService.Value.PlayerEntity = ecsWorld.Value.PackEntityWithWorld(entity);
        }

        
    }
}