using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleReportUpdatedHeros : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceOriginRef> pool = default;
        
        private readonly EcsPoolInject<IntValueComp<HpTag>> hpPool = default;

        private readonly EcsFilterInject<
            Inc<HeroInstanceOriginRef, ProcessedHeroTag>,
            Exc<DeadTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var origin = ref pool.Value.Get(entity);
                if (!origin.Packed.Unpack(out var originWorld, out var originEntity))
                    throw new Exception("No Origin");

                ref var hpComp = ref hpPool.Value.Get(entity);

                var hpPoolOrigin = originWorld.GetPool<IntValueComp<HpTag>>();
                if (hpPoolOrigin.Has(originEntity))
                {
                    ref var hpCompOrigin = ref hpPoolOrigin.Get(originEntity);
                    hpCompOrigin.Value = hpComp.Value;
                }

            }
        }
    }
}
