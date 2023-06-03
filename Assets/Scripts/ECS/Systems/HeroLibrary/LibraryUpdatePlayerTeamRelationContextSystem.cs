using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Systems
{

    public class LibraryUpdatePlayerTeamRelationContextSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<P2PRelationTag> pool = default;
        private readonly EcsPoolInject<IntValueComp<RelationScoreTag>> scorePool = default;
        private readonly EcsPoolInject<IntValueComp<RelationEffectsCountTag>> countPool = default;    
        private readonly EcsPoolInject<RelationEffectsComp> effectsPool = default;
        private readonly EcsPoolInject<RelationsMatrixComp> matrixPool = default;

        private readonly EcsFilterInject<Inc<RelationsMatrixComp>> matrixFilter = default;
        private readonly EcsFilterInject<Inc<P2PRelationTag>> filter = default;
        
        private readonly EcsFilterInject<
            Inc<PlayerTeamTag>> teamMemberFilter = default;

        private readonly EcsFilterInject<
            Inc<UpdateTag<RelationsMatrixComp>>> updateFilter = default;


        public void Run(IEcsSystems systems)
        {
            foreach (var updateEntity in updateFilter.Value)
            {
                // reset relations matrix (cleanup and refill)
                foreach (var matrixEntity in matrixFilter.Value)
                    ecsWorld.Value.DelEntity(matrixEntity);

                foreach (var p2pEntity in filter.Value)
                    ecsWorld.Value.DelEntity(p2pEntity);

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

                // fill containers for future use in the battle context
                foreach (var p2pRelEntity in filter.Value)
                {
                    ref var effectsComp = ref effectsPool.Value.Add(p2pRelEntity);
                    effectsComp .CurrentEffects = new();

                    ref var scoreComp = ref scorePool.Value.Add(p2pRelEntity);
                    scoreComp.Value = 0;
                    
                    ref var countComp = ref countPool.Value.Add(p2pRelEntity);
                    countComp.Value = 0;
                }                
            }
        }
        
    }
}

