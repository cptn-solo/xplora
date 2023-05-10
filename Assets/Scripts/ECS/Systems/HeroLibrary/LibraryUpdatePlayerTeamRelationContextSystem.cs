using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryHandleHeroMoveSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<UpdateTag<RelationsMatrixComp>> updateTagPool = default;

        private readonly EcsFilterInject<
            Inc<UpdateTag<MovedTag>, PositionComp>> moveFilter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var moveEntity in moveFilter.Value)
            {
                ref var position = ref positionPool.Value.Get(moveEntity);
                var playerTeamId = libraryService.Value.PlayerTeam.Id;

                if (position.Position.Team != position.PrevPosition.Team &&
                    (playerTeamId == position.Position.Team ||
                    playerTeamId == position.PrevPosition.Team))
                {
                    if (playerTeamTagPool.Value.Has(moveEntity))
                        playerTeamTagPool.Value.Del(moveEntity);

                    if (position.Position.Team == playerTeamId)
                        playerTeamTagPool.Value.Add(moveEntity);

                    if (libraryService.Value.PlayerTeamEntity.Unpack(out _, out var playerTeamEntity) &&
                        !updateTagPool.Value.Has(playerTeamEntity))
                        updateTagPool.Value.Add(playerTeamEntity);
                }
            }
        }
    }

    public class LibraryUpdatePlayerTeamRelationContextSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<P2PRelationTag> pool = default;
        private readonly EcsPoolInject<IntValueComp<RelationScoreTag>> scorePool = default;
        private readonly EcsPoolInject<RelationEffectsComp> effectsPool = default;
        private readonly EcsPoolInject<RelationsMatrixComp> matrixPool = default;

        private readonly EcsFilterInject<Inc<RelationsMatrixComp>> matrixFilter = default;
        private readonly EcsFilterInject<Inc<P2PRelationTag>> filter = default;
        
        private readonly EcsFilterInject<
            Inc<RelationEffectsComp>> effectsFilter = default;
        
        private readonly EcsFilterInject<
            Inc<IntValueComp<RelationScoreTag>>> scoreFilter = default;

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
                    pool.Value.Del(p2pEntity);

                foreach (var scoreEntity in scoreFilter.Value)
                    scorePool.Value.Del(scoreEntity);

                foreach (var effectsEntity in effectsFilter.Value)
                    effectsPool.Value.Del(effectsEntity);

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
                }                
            }
        }
        
    }
}

