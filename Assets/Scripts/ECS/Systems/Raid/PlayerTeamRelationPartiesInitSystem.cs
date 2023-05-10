using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamRelationPartiesInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<P2PRelationTag> pool = default;
        private readonly EcsPoolInject<RelationsMatrixComp> matrixPool = default;

        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, PlayerTeamTag>> teamMemberFilter = default;

        public void Init(IEcsSystems systems)
        {

            // fill matrix
            var matrix = new Dictionary<RelationsMatrixKey, EcsPackedEntityWithWorld>();

            foreach  (var entity1 in teamMemberFilter.Value)
            {
                var e1Packed = ecsWorld.Value.PackEntityWithWorld(entity1);

                foreach (var entity2 in teamMemberFilter.Value)
                {
                    if (entity1 == entity2)
                        continue;

                    var e2Packed = ecsWorld.Value.PackEntityWithWorld(entity2);

                    var relationEntity = ecsWorld.Value.NewEntity();            
                    pool.Value.Add(relationEntity);

                    var relPacked = ecsWorld.Value.PackEntityWithWorld(relationEntity);
                    var directKey = new RelationsMatrixKey(e1Packed, e2Packed);
                    var reverseKey = new RelationsMatrixKey(e2Packed, e1Packed);
                    if (!matrix.TryGetValue(directKey, out _))
                        matrix.Add(directKey, relPacked);

                    if (!matrix.TryGetValue(reverseKey, out _))
                        matrix.Add(reverseKey, relPacked);
                }
            };

            ref var matrixComp = ref matrixPool.Value.Add(ecsWorld.Value.NewEntity());
            matrixComp.Matrix = matrix;
        }
    }
}