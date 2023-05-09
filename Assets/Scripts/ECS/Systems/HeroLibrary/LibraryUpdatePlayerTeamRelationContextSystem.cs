using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdatePlayerTeamRelationContextSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<P2PRelationTag> pool = default;
        private readonly EcsPoolInject<RelationPartiesRef> partiesRefPool = default;       
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<IntValueComp<RelationScoreTag>> scorePool = default;
        private readonly EcsPoolInject<RelationEffectsComp> effectsPool = default;       

        private readonly EcsFilterInject<Inc<P2PRelationTag>> filter = default;
        
        private readonly EcsFilterInject<
            Inc<RelationEffectsComp>> effectsFilter = default;
        
        private readonly EcsFilterInject<
            Inc<IntValueComp<RelationScoreTag>>> scoreFilter = default;

        private readonly EcsFilterInject<
            Inc<UpdateTag<MovedTag>, PositionComp>> moveFilter = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag>> teamMemberFilter = default;

        private readonly EcsFilterInject<
            Inc<RelationPartiesRef>> p2pRefsFilter = default;

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


                    // reset relations matrix (cleanup and refill)
                    foreach (var p2pEntity in filter.Value)
                        pool.Value.Del(p2pEntity);

                    foreach (var p2pRefEntity in p2pRefsFilter.Value)
                        partiesRefPool.Value.Del(p2pRefEntity);

                    foreach (var scoreEntity in scoreFilter.Value)
                        scorePool.Value.Del(scoreEntity);

                    foreach (var effectsEntity in effectsFilter.Value)
                        effectsPool.Value.Del(effectsEntity);
                    
                    // fill matrix
                    foreach  (var entity1 in teamMemberFilter.Value)
                    {
                        ref var partiesRef = ref partiesRefPool.Value.Add(entity1);
                        partiesRef.Parties = new();
                
                        foreach (var entity2 in teamMemberFilter.Value)
                        {
                            if (entity1 == entity2)
                                continue;

                            var relationEntity = ecsWorld.Value.NewEntity();            
                            pool.Value.Add(relationEntity);

                            partiesRef.Parties.Add(
                                ecsWorld.Value.PackEntityWithWorld(entity2),
                                ecsWorld.Value.PackEntityWithWorld(relationEntity));
                        }
                    };

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
}

