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
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsFilterInject<Inc<BattleComp, BattleAftermathComp>> aftermathFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in aftermathFilter.Value)
                ProcessEcsDeathInBattle();
        }
        private void ProcessEcsDeathInBattle()
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out _))
                return;

            if (!raidService.Value.PlayerEntity.Unpack(out _, out var playerEntity))
                return;

            var heroPool = ecsWorld.Value.GetPool<HeroComp>();
            var filter = ecsWorld.Value.Filter<PlayerTeamTag>()
                .Inc<HeroConfigRefComp>()
                .Exc<DeadTag>()
                .End();

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
                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libConfigEntity))
                    throw new Exception("No hero config");

                var heroConfig = libWorld.GetPool<Hero>().Get(libConfigEntity);
                heroBuffer.Add(heroConfig);
                heroPackedBuffer.Add(heroConfigRef.RefPacked);
            }
            var bestSpeed = heroBuffer.ToArray().HeroBestBySpeed(out var idx);
            var bestSpeedPacked = heroPackedBuffer[idx];

            if (!heroPool.Has(playerEntity))
                heroPool.Add(playerEntity);

            ref var heroComp = ref heroPool.Get(playerEntity);
            heroComp.LibHeroInstance = bestSpeedPacked;

            ListPool<EcsPackedEntityWithWorld>.Add(heroPackedBuffer);
            ListPool<Hero>.Add(heroBuffer);
        }
    }
}