using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Custom upproach to the ability of pausing the world for debug purposes
    /// </summary>
    public class BaseEcsSystem : IEcsRunSystem
    {
        public virtual void RunIfActive(IEcsSystems systems)
        {

        }

        public void Run(IEcsSystems systems)
        {
            if (systems.GetShared<SharedEcsContext>().Paused)
                return;

            RunIfActive(systems);
        }
    }
}
