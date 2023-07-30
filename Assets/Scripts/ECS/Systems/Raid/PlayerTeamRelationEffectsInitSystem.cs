using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamRelationEffectsInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<RelationEffectsComp> pool = default;       

        private readonly EcsFilterInject<Inc<P2PRelationTag>> filter = default;
                
        public void Init(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var comp = ref pool.Value.Add(entity);
                comp.CurrentEffects = new();
            }
        }
    }
}