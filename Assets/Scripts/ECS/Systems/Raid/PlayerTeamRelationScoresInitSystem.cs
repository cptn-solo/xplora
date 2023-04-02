using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamRelationScoresInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<IntValueComp<RelationScoreTag>> scorePool = default;       
        private readonly EcsPoolInject<RelationScoreRef> scoreRefPool = default;       
                
        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, PlayerTeamTag>> teamMemberFilter = default;

        public void Init(IEcsSystems systems)
        {
            foreach (var entity1 in teamMemberFilter.Value)
            {
                ref var scoreRef = ref scoreRefPool.Value.Add(entity1);
                scoreRef.Parties = new();

                foreach (var entity2 in teamMemberFilter.Value)
                {
                    if (entity1 == entity2)
                        continue;

                    InitScore(entity2, ref scoreRef);
                }

            };
        }

        private void InitScore(int entity2, ref RelationScoreRef scoreRef)
        {
            var scoreEntity = ecsWorld.Value.NewEntity();
            
            scorePool.Value.Add(scoreEntity);

            scoreRef.Parties.Add(
                ecsWorld.Value.PackEntity(entity2),
                ecsWorld.Value.PackEntity(scoreEntity));
        }        
    }
}