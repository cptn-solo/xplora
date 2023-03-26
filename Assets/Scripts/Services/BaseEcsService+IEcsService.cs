using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.Services
{
    public partial class BaseEcsService : IEcsService
    {

        public void RegisterEntityViewFactory<T>(EntityViewFactory<T> factory)
            where T : struct
        {
            var factoryEntity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<EntityViewFactoryRef<T>>();
            ref var factoryRef = ref pool.Add(factoryEntity);
            factoryRef.FactoryRef = factory;
        }

        public void UnregisterEntityViewFactory<T>()
            where T : struct
        {
            if (ecsWorld == null) // if the world is destroyed already
                return;

            var filter = ecsWorld.Filter<EntityViewFactoryRef<T>>().End();
            var pool = ecsWorld.GetPool<EntityViewFactoryRef<T>>();
            foreach (var entity in filter)
                pool.Del(entity);
        }

        public bool TryGetEntityViewForPackedEntity<T, V>(
            EcsPackedEntityWithWorld? packed, out V view)
            where T : struct
        {
            view = default;

            if (packed == null || !packed.Value.Unpack(out var world, out var entity))
                return false;

            var pool = world.GetPool<EntityViewRef<T>>();
            if (!pool.Has(entity))
                return false;

            ref var entityViewRef = ref pool.Get(entity);
            view = (V)entityViewRef.EntityView;

            return true;
        }

        public void RequestDetailsHover(EcsPackedEntityWithWorld? packed)
        {
            if (packed == null || !packed.Value.Unpack(out var world, out var entity))
                return;

            var pool = world.GetPool<HoverTag>();

            if (!pool.Has(entity))
                pool.Add(entity);
        }

        public void DismissDetailsHover(EcsPackedEntityWithWorld? packed)
        {
            if (packed == null || !packed.Value.Unpack(out var world, out var entity))
                return;

            var pool = world.GetPool<HoverTag>();

            if (pool.Has(entity))
                pool.Del(entity);
        }

        public void OnEventAction<T>(int idx) where T : struct
        {
            var filter = ecsWorld.Filter<ModalDialogTag>().Inc<T>().End(); 
            var pool = ecsWorld.GetPool<ModalDialogAction<T>>();

            foreach ( var entity in filter)
            {
                ref var action = ref pool.Add(entity);
                action.ActionIdx = idx;
            }
        }
    }

}

