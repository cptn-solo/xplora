﻿using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleReportUpdatedHeros : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> pool;
        private readonly EcsPoolInject<DeadTag> deadTagPool;
        
        private readonly EcsPoolInject<HPComp> hpPool;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, HeroInstanceOriginRefComp, ProcessedHeroTag>> filter;

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
                    originWorld.DelEntity(entity);
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
