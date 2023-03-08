using System;
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
        private readonly EcsPoolInject<HeroComp> heroPool;
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

                if (battleComp.EnemyPackedEntity.Unpack(ecsWorld.Value, out var enemyEntity))
                {
                    ref var heroComp = ref heroPool.Value.Get(enemyEntity);

                    if (!heroComp.Packed.Unpack(out var libWorld, out var libEntity))
                        throw new Exception("No Hero config");

                    var playerBuffer = ListPool<EcsPackedEntityWithWorld>.Get();

                    foreach (var teamMemberEntity in teamHeroesFilter.Value)
                        playerBuffer.Add(ecsWorld.Value.PackEntityWithWorld(teamMemberEntity));

                    var enemyBuffer = ListPool<EcsPackedEntityWithWorld>.Get();

                    for (int i = 0; i < Random.Range(0, 8); i++)
                        enemyBuffer.Add(heroComp.Packed);

                    var enemyWrappedHeroes = libraryService.Value.WrapForBattle(
                        enemyBuffer.ToArray(), ecsWorld.Value);

                    battleService.Value.RequestBattle(
                        playerBuffer.ToArray(),
                        enemyWrappedHeroes);

                    ListPool<EcsPackedEntityWithWorld>.Add(playerBuffer);
                    ListPool<EcsPackedEntityWithWorld>.Add(enemyBuffer);

                }

                foreach (var unitEntity in unitFilter.Value)
                    destroyTagPool.Value.Add(unitEntity);

                //same entity as unit but for clarity let's address it as overlayEntity
                foreach (var overlayEntity in overlayFilter.Value) 
                    destroyTagPool.Value.Add(overlayEntity);

                raidService.Value.StartBattle();
            }

        }
    }
}