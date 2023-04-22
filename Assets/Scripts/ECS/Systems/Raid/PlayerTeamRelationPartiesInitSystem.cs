using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamRelationPartiesInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<P2PRelationTag> pool = default;
        private readonly EcsPoolInject<RelationPartiesRef> partiesRefPool = default;       

        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, PlayerTeamTag>> teamMemberFilter = default;

        public void Init(IEcsSystems systems)
        {
            foreach (var entity1 in teamMemberFilter.Value)
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
                        ecsWorld.Value.PackEntity(entity2),
                        ecsWorld.Value.PackEntity(relationEntity));
                }
            };
        }
    }
}