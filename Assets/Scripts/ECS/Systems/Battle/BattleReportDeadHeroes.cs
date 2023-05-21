using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleReportDeadHeroes : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> pool = default;
                
        private readonly EcsFilterInject<
            Inc<HeroInstanceOriginRefComp, ProcessedHeroTag, DeadTag>
            > filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var origin = ref pool.Value.Get(entity);
                if (!origin.Packed.Unpack(out var originWorld, out var originEntity))
                    throw new Exception("No Origin");

                var deadPool = originWorld.GetPool<DeadTag>();
                if (!deadPool.Has(originEntity))
                    deadPool.Add(originEntity);
                
                var matrixFilter = originWorld.Filter<RelationsMatrixComp>().End();
                var matrixPool = originWorld.GetPool<RelationsMatrixComp>();

                foreach (var matrixEntity in matrixFilter)
                {
                    ref var matrixComp = ref matrixPool.Get(matrixEntity);
                    
                    var buff = ListPool<RelationsMatrixKey>.Get();

                    var matrix = matrixComp.Matrix;

                    foreach (var item in matrix)
                    {
                        if (item.Key.Item1.EqualsTo(origin.Packed) ||
                            item.Key.Item2.EqualsTo(origin.Packed))
                            buff.Add(item.Key);
                    }

                    if (buff.Count > 0)
                        foreach (var key in buff)
                            matrix.Remove(key);

                    matrixComp.Matrix = matrix;

                    ListPool<RelationsMatrixKey>.Add(buff);
                    
                }
            }
        }
    }
}
