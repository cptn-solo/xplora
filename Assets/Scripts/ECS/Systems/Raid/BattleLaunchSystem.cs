using System;
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
    public class BattleLaunchSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<BattleComp> battlePool;
        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<StrengthComp> strengthPool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;

        private readonly EcsFilterInject<Inc<BattleComp, DraftTag>> battleFilter;
        private readonly EcsFilterInject<Inc<EntityViewRef<Hero>>> unitFilter;
        private readonly EcsFilterInject<Inc<EntityViewRef<bool>>> overlayFilter;
        private readonly EcsFilterInject<Inc<PlayerTeamTag, HeroConfigRefComp>> teamHeroesFilter;

        private readonly EcsCustomInject<RaidService> raidService;
        private readonly EcsCustomInject<HeroLibraryService> libraryService;
        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in battleFilter.Value)
            {
                ref var battleComp = ref battlePool.Value.Get(entity);

                if (!battleComp.EnemyPackedEntity.Unpack(ecsWorld.Value, out var enemyEntity))
                    throw new Exception("No Enemy");

                if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                    throw new Exception("No Raid");

                var playerBuffer = ListPool<EcsPackedEntityWithWorld>.Get();

                foreach (var teamMemberEntity in teamHeroesFilter.Value)
                    playerBuffer.Add(ecsWorld.Value.PackEntityWithWorld(teamMemberEntity));

                var enemyBuffer = ListPool<EcsPackedEntityWithWorld>.Get();

                ref var raidComp = ref raidPool.Value.Get(raidEntity);
                ref var strengthComp = ref strengthPool.Value.Get(enemyEntity);

                var config = OpponentTeamMemberSpawnConfig.DefaultConfig;

                var initialStrength = strengthComp.Value;
                var memberCount = 0;

                var sortedByStrength = config.OveralStrengthLevels.OrderByDescending(x => x.Key);

                while (initialStrength > 0 && memberCount < 8)
                {
                    foreach (var item in sortedByStrength)
                    {
                        if (item.Key > strengthComp.Value)
                            continue;

                        var adjustedSpawnRate = item.Value.SpawnRate;

                        foreach (var adj in item.Value.TeamStrengthWeightedSpawnRates)
                        {
                            if (adj.Key.Item1 < strengthComp.Value &&
                                adj.Key.Item2 >= strengthComp.Value)
                            {
                                adjustedSpawnRate += adj.Value;
                                break;
                            }
                        }

                        if (!adjustedSpawnRate.RatedRandomBool())
                            continue;

                        var spawnOptions = raidComp.OpponentsIndexedByStrength[item.Key];
                        var spawned = spawnOptions[Random.Range(0, spawnOptions.Count)];

                        enemyBuffer.Add(spawned);
                        initialStrength -= item.Key;
                        memberCount++;
                    }
                }

                var enemyWrappedHeroes = libraryService.Value.WrapForBattle(
                    enemyBuffer.ToArray(), ecsWorld.Value);

                battleService.Value.RequestBattle(
                    playerBuffer.ToArray(),
                    enemyWrappedHeroes);

                ListPool<EcsPackedEntityWithWorld>.Add(playerBuffer);
                ListPool<EcsPackedEntityWithWorld>.Add(enemyBuffer);
                raidService.Value.StartBattle();
            }

        }
    }
}