using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    using HeroIndexByStrength = Dictionary<int, List<EcsPackedEntityWithWorld>>;

    public class OpponentInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<OpponentComp> opponentPool;
        private readonly EcsPoolInject<StrengthComp> strengthPool;
        
        private readonly EcsPoolInject<HeroComp> heroPool;

        private readonly EcsCustomInject<RaidService> raidService;
        private readonly EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            ref var raidComp = ref raidPool.Value.Get(raidEntity);

            var enemySpawnConfig = OpponentSpawnConfig.DefaultConfig;

            // % of free world cells we can occupy by enemies:
            var enemySpawnFactor = Random.Range(
                enemySpawnConfig.SpawnRateForWorldChunk.Item1,
                enemySpawnConfig.SpawnRateForWorldChunk.Item2);

            // attributes ignored:
            var availableCellsCount = worldService.Value.GetFreeCellsCount(true);

            var enemyNumber = (int)((float)availableCellsCount * (float)enemySpawnFactor / 100f);
            Debug.Log($"enemyNumber: {enemyNumber}");

            var indexed = IndexHeroesByStrength(raidComp.OpponentHeroConfigs);
            raidComp.OpponentsIndexedByStrength = indexed;

            for (int idx = 0; idx < enemyNumber; idx++)
            {
                var teamStrength = enemySpawnConfig.RandomRangedTeamStrength(Random.Range(1, 101));
                Debug.Log($"teamStrength: {teamStrength}");
                var opponentEntity = ecsWorld.Value.NewEntity();

                ref var opponentComp = ref opponentPool.Value.Add(opponentEntity);

                ref var strength = ref strengthPool.Value.Add(opponentEntity);
                strength.Value = teamStrength;

                ref var heroComp = ref heroPool.Value.Add(opponentEntity);                
                heroComp.Hero = StrongestHeroForTeamStrength(teamStrength, indexed);
            }
        }

        private EcsPackedEntityWithWorld StrongestHeroForTeamStrength(
            int teamStrength, HeroIndexByStrength indexed)
        {
            foreach (var item in indexed.Keys.OrderByDescending(x => x))
            {
                if (teamStrength < item)
                    continue;

                var options = indexed[item];
                return options[Random.Range(0, options.Count)];
            }
            throw new Exception($"No available options for team strength {teamStrength}");
        }

        private static HeroIndexByStrength IndexHeroesByStrength(EcsPackedEntityWithWorld[] configs)
        {
            var heroIndexByStrength = new Dictionary<int, List<EcsPackedEntityWithWorld>>();
            foreach (var item in configs)
            {
                if (!item.Unpack(out var libWorld, out var entity))
                    throw new Exception("No Hero");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(entity);
                if (!heroIndexByStrength.TryGetValue(heroConfig.OveralStrength, out var buffer))
                {
                    buffer = ListPool<EcsPackedEntityWithWorld>.Get();
                    buffer.Add(item);
                    heroIndexByStrength.Add(heroConfig.OveralStrength, buffer);
                }
                else
                {
                    buffer.Add(item);
                    heroIndexByStrength[heroConfig.OveralStrength] = buffer;
                }
            }
            return heroIndexByStrength;
        }
    }
}