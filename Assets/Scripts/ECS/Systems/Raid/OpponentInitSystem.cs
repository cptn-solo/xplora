﻿using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class OpponentInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<OpponentComp> opponentPool;
        private readonly EcsPoolInject<HeroComp> heroPool;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Init(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            ref var raidComp = ref raidPool.Value.Get(raidEntity);

            foreach (var opponentHero in raidComp.OpponentHeroes)
            {
                var opponentEntity = ecsWorld.Value.NewEntity();
                ref var opponentComp = ref opponentPool.Value.Add(opponentEntity);
                ref var heroComp = ref heroPool.Value.Add(opponentEntity);
                heroComp.Hero = opponentHero;
            }
        }
    }
}