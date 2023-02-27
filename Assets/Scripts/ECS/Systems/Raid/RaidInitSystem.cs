using System.Collections.Generic;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Init(IEcsSystems systems)
        {
            var success = raidService.Value.AssignPlayerAndEnemies(
                out var playerHeroes,
                out var opponentHeroes);

            var raidEntity = ecsWorld.Value.NewEntity();

            ref var raidComp = ref raidPool.Value.Add(raidEntity);
            raidComp.PlayerHeroConfigs = playerHeroes;
            raidComp.OpponentHeroConfigs = opponentHeroes;

            raidService.Value.RaidEntity = ecsWorld.Value.PackEntity(raidEntity);
        }
    }
}