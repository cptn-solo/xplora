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

        private readonly EcsCustomInject<RaidService> raidService;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in battleFilter.Value)
            {
                ref var battleComp = ref battlePool.Value.Get(entity);

                if (battleComp.EnemyPackedEntity.Unpack(ecsWorld.Value, out var enemyEntity))
                {
                    ref var heroComp = ref heroPool.Value.Get(enemyEntity);
                    raidService.Value.MoveEnemyToFront(heroComp.Hero);
                }

                foreach (var unitEntity in unitFilter.Value)
                    destroyTagPool.Value.Add(unitEntity);

                raidService.Value.StartBattle();
            }

        }
    }
}