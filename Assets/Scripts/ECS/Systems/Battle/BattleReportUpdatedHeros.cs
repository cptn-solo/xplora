using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleReportUpdatedHeros : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> pool = default;
        private readonly EcsPoolInject<DeadTag> deadTagPool = default;
        
        private readonly EcsPoolInject<HPComp> hpPool = default;

        private readonly EcsFilterInject<
            Inc<HeroInstanceOriginRefComp, ProcessedHeroTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var origin = ref pool.Value.Get(entity);
                if (!origin.Packed.Unpack(out var originWorld, out var originEntity))
                    throw new Exception("No Origin");

                ref var hpComp = ref hpPool.Value.Get(entity);

                if (deadTagPool.Value.Has(entity))
                {
                    originWorld.DelEntity(originEntity);
                    continue;
                }

                var hpPoolOrigin = originWorld.GetPool<HPComp>();
                if (hpPoolOrigin.Has(originEntity))
                {
                    ref var hpCompOrigin = ref hpPoolOrigin.Get(originEntity);
                    hpCompOrigin.Value = hpComp.Value;
                }

            }
        }
    }
}
