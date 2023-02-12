using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RetireEnemySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<GarbageTag> garbagePool;

        private readonly EcsFilterInject<Inc<OpponentComp, RetireTag>> opponentToRetireFilter;

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