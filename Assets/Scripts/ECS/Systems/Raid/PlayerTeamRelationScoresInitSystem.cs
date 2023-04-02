using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamRelationScoresInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<RelationScoreComp> relationScorePool = default;       
                
        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, PlayerTeamTag>> teamMemberFilter = default;

        public void Init(IEcsSystems systems)
        {
            foreach (var entity1 in teamMemberFilter.Value)
            {
                foreach (var entity2 in teamMemberFilter.Value)
                {
                    if (entity1 == entity2)
                        continue;

                    InitScore(entity1, entity2);
                }

            };
        }

        private void InitScore(int entity1, int entity2)
        {
            var scoreEntity = ecsWorld.Value.NewEntity();
            ref var scoreComp = ref relationScorePool.Value.Add(scoreEntity);
            scoreComp.Parties = new EcsPackedEntity []
            {
                ecsWorld.Value.PackEntity(entity1),
                ecsWorld.Value.PackEntity(entity2),
            };
        }        
    }
}