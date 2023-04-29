using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueExpiredRelationEffectsSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<BattleRoundInfo, GarbageTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                DequeueEffect(entity);
        }

        private void DequeueEffect(int entity)
        {
            //TODO: 1. scan all current effects in RelationEffectsComp
            // for both battle and raid worlds and remove expired effects.
            // 2. send update signal for views UpdateTag<RelationEffectInfo>
            // 3. remove expired effect
        }

    }
}
