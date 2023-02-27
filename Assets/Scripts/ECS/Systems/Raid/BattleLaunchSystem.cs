using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleLaunchSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<BattleComp> battlePool;
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;

        private readonly EcsFilterInject<Inc<BattleComp, DraftTag>> battleFilter;
        private readonly EcsFilterInject<Inc<UnitRef>> unitFilter;
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

                    var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

                    foreach (var teamMemberEntity in teamHeroesFilter.Value)
                        buffer.Add(ecsWorld.Value.PackEntityWithWorld(teamMemberEntity));

                    var enemyWrappedHeroes = libraryService.Value.WrapForBattle(new[] { heroComp.Packed });

                    battleService.Value.RequestBattle(
                        buffer.ToArray(),
                        enemyWrappedHeroes);

                    ListPool<EcsPackedEntityWithWorld>.Add(buffer);

                }

                foreach (var unitEntity in unitFilter.Value)
                    destroyTagPool.Value.Add(unitEntity);

                raidService.Value.StartBattle();
            }

        }
    }
}