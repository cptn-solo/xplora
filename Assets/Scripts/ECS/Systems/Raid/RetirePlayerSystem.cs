using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class RetirePlayerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<GarbageTag> garbagePool = default;

        private readonly EcsFilterInject<Inc<PlayerComp, RetireTag>> playerToRetireFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in playerToRetireFilter.Value)
                garbagePool.Value.Add(entity);
        }
    }
}