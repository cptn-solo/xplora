using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamRelationScoresInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<IntValueComp<RelationScoreTag>> pool = default;    
        private readonly EcsPoolInject<IntValueComp<RelationEffectsCountTag>> countPool = default;    

        private readonly EcsFilterInject<Inc<P2PRelationTag>> filter = default;
                
        public void Init(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                pool.Value.Add(entity);         
                countPool.Value.Add(entity);
            }
        }
    }
}