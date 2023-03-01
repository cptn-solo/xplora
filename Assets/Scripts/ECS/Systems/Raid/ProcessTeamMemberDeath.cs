using Assets.Scripts.Data;
using System;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class ProcessTeamMemberDeath : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsFilterInject<Inc<BattleComp, BattleAftermathComp>> aftermathFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in aftermathFilter.Value)
            {
                ProcessEcsDeathInBattle(entity);
            }
        }
        private void ProcessEcsDeathInBattle(int entity)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            if (!raidService.Value.PlayerEntity.Unpack(ecsWorld.Value, out var playerEntity))
                return;

            var heroPool = ecsWorld.Value.GetPool<HeroComp>();
            var filter = ecsWorld.Value.Filter<PlayerTeamTag>().Inc<HeroConfigRefComp>().End();

            if (filter.GetEntitiesCount() == 0)
            {
                heroPool.Del(playerEntity);
                return;
            }

            var heroBuffer = ListPool<Hero>.Get();
            var heroPackedBuffer = ListPool<EcsPackedEntityWithWorld>.Get();

            foreach (var heroInstanceEntity in filter)
            {
                var heroConfigRefPool = ecsWorld.Value.GetPool<HeroConfigRefComp>();
                ref var heroConfigRef = ref heroConfigRefPool.Get(heroInstanceEntity);

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                var heroConfigPool = libWorld.GetPool<Hero>();
                ref var heroConfig = ref heroConfigPool.Get(libEntity);
                heroBuffer.Add(heroConfig);
                heroPackedBuffer.Add(heroConfigRef.Packed);
            }
            var bestSpeed = heroBuffer.ToArray().HeroBestBySpeed(out var idx);
            var bestSpeedPacked = heroPackedBuffer[idx];

            if (!heroPool.Has(playerEntity))
                heroPool.Add(playerEntity);

            ref var heroComp = ref heroPool.Get(playerEntity);

            heroComp.Hero = bestSpeedPacked;

            ListPool<EcsPackedEntityWithWorld>.Add(heroPackedBuffer);
            ListPool<Hero>.Add(heroBuffer);
        }
    }
}