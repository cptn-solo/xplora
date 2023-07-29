using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Custom upproach to the ability of pausing the world for debug purposes
    /// </summary>
    public class CleanupSystem<T> : BaseEcsSystem where T : struct
    {
        readonly EcsFilter _filter;
        readonly EcsPool<T> _pool;

        public CleanupSystem(EcsWorld world)
        {
            _filter = world.Filter<T>().End();
            _pool = world.GetPool<T>();
        }

        public override void RunIfActive(IEcsSystems systems)
        {            
            foreach (var entity in _filter)
            {
                _pool.Del(entity);
            }
        }
    }
}
