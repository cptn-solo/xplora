using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<TeamComp> teamPool;
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<PowerComp> powerPool;
        private readonly EcsPoolInject<StaminaComp> staminaPool;
        private readonly EcsPoolInject<SightRangeComp> sightRangePool;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Init(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            ref var raidComp = ref raidPool.Value.Get(raidEntity);

            if (raidComp.InitialPlayerHeroes.Length <= 0)
                return;

            var entity = ecsWorld.Value.NewEntity();

            ref var playerComp = ref playerPool.Value.Add(entity);

            ref var teamComp = ref teamPool.Value.Add(entity);

            ref var heroComp = ref heroPool.Value.Add(entity);
            heroComp.Hero = raidComp.InitialPlayerHeroes.HeroBestBySpeed();

            ref var sightRangeComp = ref sightRangePool.Value.Add(entity);
            sightRangeComp.Range = 2;

            ref var staminaComp = ref staminaPool.Value.Add(entity);

            var initialPower = 0;
            foreach (var hero in raidComp.InitialPlayerHeroes)
                initialPower += 5 * hero.Health * hero.Speed / raidComp.InitialPlayerHeroes.Length; ;

            ref var powerComp = ref powerPool.Value.Add(entity);
            powerComp.InitialValue = initialPower;
            powerComp.CurrentValue = initialPower;

            raidService.Value.PlayerEntity = ecsWorld.Value.PackEntity(entity);
        }
    }
}