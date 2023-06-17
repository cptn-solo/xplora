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
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<RaidComp> raidPool = default;
        private readonly EcsPoolInject<OpponentComp> opponentPool = default;
        private readonly EcsPoolInject<IntValueComp<StrengthTag>> strengthPool = default;
        
        private readonly EcsPoolInject<HeroComp> heroPool = default;

        private readonly EcsCustomInject<RaidService> raidService = default;
        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Init(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            ref var raidComp = ref raidPool.Value.Get(raidEntity);

            ref var enemySpawnConfig = ref raidService.Value.OpponentSpawnConfig();

            // % of free world cells we can occupy by enemies:
            var enemySpawnFactor = Random.Range(
                enemySpawnConfig.SpawnRateForWorldChunk.MinRate,
                enemySpawnConfig.SpawnRateForWorldChunk.MaxRate);

            // attributes ignored:
            var availableCellsCount = worldService.Value.GetFreeCellsCount(true);

            var enemyNumber = (int)((float)availableCellsCount * (float)enemySpawnFactor / 100f);
            Debug.Log($"enemyNumber: {enemyNumber}");

            var indexed = IndexHeroRefsByStrength(raidComp.OpponentLibHeroInstances);
            ref var config = ref raidService.Value.OpponentTeamMemberSpawnConfig();
            raidComp.OpponentsIndexedByStrength = indexed;
            raidComp.OppenentMembersSpawnConfig = config;
            
            for (int idx = 0; idx < enemyNumber; idx++)
            {
                var teamStrength = enemySpawnConfig.RandomRangedTeamStrength(Random.Range(1, 101));

                var opponentEntity = ecsWorld.Value.NewEntity();

                var coverHeroStrenth = CoverHeroForTeamStrength(teamStrength, indexed, config);

                var spawnOptions = indexed[coverHeroStrenth];
                var spawned = spawnOptions[Random.Range(0, spawnOptions.Count)];

                ref var strength = ref strengthPool.Value.Add(opponentEntity);
                strength.Value = teamStrength;

                ref var heroComp = ref heroPool.Value.Add(opponentEntity);
                heroComp.LibHeroInstance = spawned;

                ref var opponentComp = ref opponentPool.Value.Add(opponentEntity);
                opponentComp.CoverHeroStrength = coverHeroStrenth;
            }
        }

        private int CoverHeroForTeamStrength(
            int teamStrength, HeroIndexByStrength indexed,
            OpponentTeamMemberSpawnConfig config)
        {
            if (config.RunOnePass(teamStrength) is var strength && strength > 0)
                return strength;

            return StrongestHeroForTeamStrength(teamStrength, indexed);
        }

        /// <summary>
        /// Fallback method of cover hero selection - just picks 1st hero who's
        /// strength is smaller than overal team strength (while the list is sorted
        /// descending the strength so most strong heroes go 1st)
        /// </summary>
        /// <param name="teamStrength"></param>
        /// <param name="indexed">heroes indexed by their respective strength</param>
        /// <returns>strength value used as a key in the hero index</returns>
        /// <exception cref="Exception"></exception>
        private int StrongestHeroForTeamStrength(
            int teamStrength, HeroIndexByStrength indexed)
        {
            foreach (var item in indexed.Keys.OrderByDescending(x => x))
                if (teamStrength >= item)
                    return item;

            throw new Exception($"No available options for team strength {teamStrength}");
        }

        private static HeroIndexByStrength IndexHeroRefsByStrength(EcsPackedEntityWithWorld[] configRefs)
        {
            var heroIndexByStrength = new HeroIndexByStrength();
            foreach (var configRefPacked in configRefs)
            {
                if (!configRefPacked.Unpack(out var libWorld, out var configRefEntity))
                    throw new Exception("No Hero config ref");

                var configRefPool = libWorld.GetPool<HeroConfigRef>();
                ref var configRef = ref configRefPool.Get(configRefEntity);

                if (!configRef.Packed.Unpack(out _, out var entity))
                    throw new Exception("No Hero config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(entity);
                if (!heroIndexByStrength.TryGetValue(heroConfig.OveralStrength, out var buffer))
                {
                    buffer = ListPool<EcsPackedEntityWithWorld>.Get();
                    buffer.Add(configRefPacked);
                    heroIndexByStrength.Add(heroConfig.OveralStrength, buffer);
                }
                else
                {
                    buffer.Add(configRefPacked);
                    heroIndexByStrength[heroConfig.OveralStrength] = buffer;
                }
            }
            return heroIndexByStrength;
        }
    }
}