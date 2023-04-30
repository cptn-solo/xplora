using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RetireEnemySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<GarbageTag> garbagePool = default;

        private readonly EcsFilterInject<Inc<OpponentComp, RetireTag>> opponentToRetireFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in opponentToRetireFilter.Value)
            {
                if (!garbagePool.Value.Has(entity))
                    garbagePool.Value.Add(entity);
            }
        }
    }
}