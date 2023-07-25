using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleSceneVisualSystem<T, V> : BattleSceneVisualSystem<T>
        where T : struct, ISceneVisualsInfo
        where V : struct
    {
        protected readonly EcsPoolInject<EntityViewRef<V>> viewRefPool = default;

        protected override void AssignVisualizer(int entity, T visualInfo, EcsWorld world, int viewEntity)
        {
            ref var viewRef = ref viewRefPool.Value.Get(viewEntity);
            var view = viewRef.EntityView;
            view.Visualize<T>(visualInfo, world.PackEntityWithWorld(entity));
        }
    }

    public class BattleSceneVisualSystem<T> : IEcsRunSystem
        where T : struct, ISceneVisualsInfo
    {
        protected readonly EcsPoolInject<T> visualPool = default;
        protected readonly EcsPoolInject<ActiveVisualsTag> activeVisualsTagPool = default;

        protected readonly EcsFilterInject<
            Inc<T, RunningVisualsTag>,
            Exc<ActiveVisualsTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                activeVisualsTagPool.Value.Add(entity);

                ref var visualInfo = ref visualPool.Value.Get(entity);
                if (visualInfo.SubjectEntity.Unpack(out var world, out var viewEntity) &&
                    world.Equals(systems.GetWorld()))
                {
                    // will delete entity if no visualizer implemented
                    AssignVisualizer(entity, visualInfo, world, viewEntity);
                }
            }
        }
        protected virtual void AssignVisualizer(int entity, T visualInfo, EcsWorld world, int viewEntity)
        {
            // base implementation is just mark visual as complete by removing it's entity from the world:
            // override if there is some animation to be played for the visualInfo
            // or call immediately after view update
            world.GetPool<GarbageTag>().Add(entity);

        }

    }
}
