using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class RetirePlayerSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<GarbageTag> garbagePool = default;

        private readonly EcsFilterInject<Inc<PlayerComp, RetireTag>> playerToRetireFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in playerToRetireFilter.Value)
                garbagePool.Value.Add(entity);
        }
    }
}