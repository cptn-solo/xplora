using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateBarsInfoSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BarsInfoComp> barsInfoPool = default;
        private readonly EcsPoolInject<UpdateTag<BarsInfoComp>> updatePool = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, UpdateTag<BarsInfoComp>>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                var world = filter.Value.GetWorld();

                ref var barsInfoComp = ref barsInfoPool.Value.Get(entity);

                barsInfoComp.DamageMax = world.ReadIntRangeValue<DamageRangeTag>(entity).MaxRate;
                barsInfoComp.Health = world.ReadIntValue<HealthTag>(entity);
                barsInfoComp.Speed = world.ReadIntValue<SpeedTag>(entity);
                barsInfoComp.DefenceRate = world.ReadIntValue<DefenceRateTag>(entity);
                barsInfoComp.AccuracyRate = world.ReadIntValue<AccuracyRateTag>(entity);
                barsInfoComp.DodgeRate = world.ReadIntValue<DodgeRateTag>(entity); ;
                barsInfoComp.CriticalHitRate = world.ReadIntValue<CritRateTag>(entity); ;

                barsInfoComp.Generate();

                updatePool.Value.Del(entity);
            }
        }
    }
}