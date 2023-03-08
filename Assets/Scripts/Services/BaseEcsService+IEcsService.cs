using Assets.Scripts.ECS.Data;

namespace Assets.Scripts.Services
{
    public partial class BaseEcsService : IEcsService
    {

        public void RegisterEntityViewFactory<T>(EntityViewFactory<T> factory) where T : struct
        {
            var factoryEntity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<EntityViewFactoryRef<T>>();
            ref var factoryRef = ref pool.Add(factoryEntity);
            factoryRef.FactoryRef = factory;
        }

        public void UnregisterEntityViewFactory<T>() where T : struct
        {
            if (ecsWorld == null) // if the world is destroyed already
                return;

            var filter = ecsWorld.Filter<EntityViewFactoryRef<T>>().End();
            var pool = ecsWorld.GetPool<EntityViewFactoryRef<T>>();
            foreach (var entity in filter)
                pool.Del(entity);
        }
    }

}

