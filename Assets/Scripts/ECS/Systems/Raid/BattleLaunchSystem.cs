using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleLaunchSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<BattleComp> battlePool;
        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<StrengthComp> strengthPool;
        private readonly EcsPoolInject<OpponentComp> opponentPool;
        private readonly EcsPoolInject<HeroComp> heroCompPool;
        private readonly EcsFilterInject<Inc<BattleComp, DraftTag>> battleFilter;
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
                ref var opponentComp = ref opponentPool.Value.Get(enemyEntity);
                ref var heroComp = ref heroCompPool.Value.Get(enemyEntity);

                enemyBuffer.Add(heroComp.Hero);

                int teamStrength = strengthComp.Value;
                var remainingStrength = teamStrength - opponentComp.CoverHeroStrength;
                var memberCount = 1;
                var indexed = raidComp.OpponentsIndexedByStrength;

                void callback(int strength)
                {
                    var spawnOptions = indexed[strength];
                    var spawned = spawnOptions[Random.Range(0, spawnOptions.Count)];

                    enemyBuffer.Add(spawned);
                    remainingStrength -= strength;
                    memberCount++;
                }

                while (remainingStrength > 0 && memberCount < 8)
                    raidComp.OppenentMembersSpawnConfig.RunOnePass(
                        remainingStrength, callback, opponentComp.CoverHeroStrength);

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