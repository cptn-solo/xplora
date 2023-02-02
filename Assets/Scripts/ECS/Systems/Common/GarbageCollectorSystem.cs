using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class GarbageCollectorSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;
        private readonly EcsFilterInject<Inc<DestroyTag>> garbageTagFilter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entityToDestroy in garbageTagFilter.Value)
                ecsWorld.Value.DelEntity(entityToDestroy);
        }
    }
}