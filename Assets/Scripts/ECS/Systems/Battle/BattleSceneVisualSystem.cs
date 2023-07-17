using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleSceneVisualSystem<T, V> : IEcsRunSystem
        where T : struct, ISceneVisualsInfo
        where V : struct
    {
        protected readonly EcsPoolInject<T> visualPool = default;
        protected readonly EcsPoolInject<ActiveVisualsTag> pool = default;
        protected readonly EcsPoolInject<EntityViewRef<V>> pool1 = default;

        protected readonly EcsFilterInject<
            Inc<T, RunningVisualsTag>,
            Exc<ActiveVisualsTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var visualInfo = ref visualPool.Value.Get(entity);
                if (visualInfo.SubjectEntity.Unpack(out var world, out var viewEntity) &&
                    world.Equals(systems.GetWorld()))
                {
                    AssignVisualizer(entity, visualInfo, world, viewEntity);
                }

                pool.Value.Add(entity);
            }
        }

        protected virtual void AssignVisualizer(int entity, T visualInfo, EcsWorld world, int viewEntity)
        {
            ref var viewRef = ref pool1.Value.Get(viewEntity);
            var view = viewRef.EntityView;
            view.Visualize<T>(visualInfo, world.PackEntityWithWorld(entity));
        }
    }
}
