using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCleanupCompletedRoundSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;


        private readonly EcsPoolInject<BattleRoundInfo> pool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;

        private readonly EcsFilterInject<Inc<BattleRoundInfo, DestroyTag>> filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                pool.Value.Del(entity);
                destroyTagPool.Value.Del(entity);
            }
        }
    }
}
