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
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<TeamComp> teamPool;
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<StaminaComp> staminaPool;
        private readonly EcsPoolInject<SightRangeComp> sightRangePool;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<SpeedComp> speedCompPool;
        private readonly EcsPoolInject<HealthComp> healthCompPool;
        private readonly EcsPoolInject<HPComp> hpCompPool;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Init(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            ref var raidComp = ref raidPool.Value.Get(raidEntity);

            if (raidComp.PlayerHeroConfigs.Length <= 0)
                return;

            var entity = ecsWorld.Value.NewEntity();

            ref var playerComp = ref playerPool.Value.Add(entity);

            ref var teamComp = ref teamPool.Value.Add(entity);

            var heroBuffer = ListPool<Hero>.Get();

            heroBuffer.AddRange(raidComp.PlayerHeroConfigs.Select((x) => {
                x.Unpack(out var libWorld, out var libHeroConfig);
                var heroPool = libWorld.GetPool<Hero>();
                ref var heroConfig = ref heroPool.Get(libHeroConfig);
                return heroConfig;
            }));
            var bestSpeed = heroBuffer.ToArray().HeroBestBySpeed(out var idx);

            ref var heroComp = ref heroPool.Value.Add(entity);
            heroComp.Hero = raidComp.PlayerHeroConfigs[idx];

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
            for (int i = 0; i < raidComp.PlayerHeroConfigs.Length; i++)
            {
                var heroConfigPackedEntity = raidComp.PlayerHeroConfigs[i];
                var playerTeamMemberEntity = ecsWorld.Value.NewEntity();
                playerTeamTagPool.Value.Add(playerTeamMemberEntity);

                ref var heroConfigRef = ref heroConfigRefPool.Value.Add(playerTeamMemberEntity);
                heroConfigRef.HeroConfigPackedEntity = heroConfigPackedEntity;

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(libEntity);

                ref var speedComp = ref speedCompPool.Value.Add(playerTeamMemberEntity);
                speedComp.Value = heroConfig.Speed;

                ref var healthComp = ref healthCompPool.Value.Add(playerTeamMemberEntity);
                healthComp.Value = heroConfig.Health;

                ref var hpComp = ref hpCompPool.Value.Add(playerTeamMemberEntity);
                hpComp.Value = (int)(heroConfig.Health / 2);
            }

            ListPool<Hero>.Add(heroBuffer);

            raidService.Value.PlayerEntity = ecsWorld.Value.PackEntityWithWorld(entity);
        }
    }
}