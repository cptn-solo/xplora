using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using System;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public partial class BaseEcsService : IEcsService
    {
        public event UnityAction OnManagedSceneDecomission;
        
        public void DestroyEcsEntityViews() =>
            OnManagedSceneDecomission?.Invoke();

        public void RegisterEntityView<T>(
            IEntityView<T> entityView)
            where T : struct
        {
            var entity = ecsWorld.NewEntity();
            RegisterEntityView<T>(entityView, ecsWorld.PackEntityWithWorld(entity));
        }
        public void RegisterEntityView<T>(
            IEntityView<T> entityView, EcsPackedEntity packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(ecsWorld, out var entity))
                throw new Exception("No Entity");

            RegisterEntityView<T>(entityView, ecsWorld.PackEntityWithWorld(entity));
        }

        public void RegisterEntityView<T>(
            IEntityView<T> entityView, EcsPackedEntityWithWorld packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No Entity");

            entityView.PackedEntity = packedEntity;
            entityView.DataLoader = GetDataForPackedEntity<T>;

            var pool = world.GetPool<EntityViewRef<T>>();
            if (!pool.Has(entity))
                pool.Add(entity);

            ref var entityViewRef = ref pool.Get(entity);
            entityViewRef.EntityView = entityView;

            entityView.EcsService = this;
            OnManagedSceneDecomission += entityView.Decommision;
        }
        

        public bool TryGetEntityView<T>(EcsPackedEntityWithWorld packedEntity,
            out IEntityView<T> view)
            where T : struct
        {
            view = null;

            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No Entity");

            var pool = world.GetPool<EntityViewRef<T>>();

            if (!pool.Has(entity))
                throw new Exception("No Entity view ref");

            ref var entityViewRef = ref pool.Get(entity);
            view = entityViewRef.EntityView;

            return true;
        }
        public void RegisterEntityButtonRef<T>(IEntityButton<T> refOrigin)
        {
            if (ecsWorld == null)
                return;

            var entity = ecsWorld.NewEntity();

            ref var buttonRef = ref EcsWorld.GetPool<EntityButtonRef<T>>().Add(entity);
            buttonRef.EntityButton = refOrigin;
            buttonRef.Transform = refOrigin.Transform;
            refOrigin.PackedEntity = EcsWorld.PackEntityWithWorld(entity);
        }

        public void UnregisterEntityButtonRef<T>(IEntityButton<T> refOrigin)
        {
            if (ecsWorld == null)
                return;

            var filter = ecsWorld.Filter<EntityButtonRef<T>>().End();
            var pool = ecsWorld.GetPool<EntityButtonRef<T>>();

            var garbage = ecsWorld.GetPool<GarbageTag>();

            foreach (var entity in filter)
            {
                ref var buttonRef = ref pool.Get(entity);
                buttonRef.EntityButton = null;
                buttonRef.Transform = null;
                garbage.Add(entity); // so some other system can
                                     // remove other dependencies
                                     // (if any)

            }
        }



        public void RegisterTransformRef<T>(ITransform<T> transformRefOrigin)
        {
            if (ecsWorld == null)
                return;

            var entity = ecsWorld.NewEntity();
            ref var transformRef = ref ecsWorld.GetPool<TransformRef<T>>().Add(entity);
            transformRef.Transform = transformRefOrigin.Transform;
        }

        public void UnregisterTransformRef<T>(ITransform transformRefOrigin)
        {
            if (ecsWorld == null)
                return;

            var filter = ecsWorld.Filter<TransformRef<T>>().End();
            var pool = ecsWorld.GetPool<TransformRef<T>>();
            var garbage = ecsWorld.GetPool<GarbageTag>();

            foreach (var entity in filter)
            {
                ref var transformRef = ref pool.Get(entity);
                transformRef.Transform = null;
                garbage.Add(entity); // so some other system can
                                     // remove other dependencies
                                     // (if any)

            }
        }

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
        internal T GetDataForPackedEntity<T>(EcsPackedEntityWithWorld? packed)
            where T : struct
        {
            if (packed != null && packed.Value.Unpack(out var world, out var entity))
                return world.GetPool<T>().Get(entity);

            return default;
        }

        internal void EnqueueEntityViewUpdate<T>(EcsPackedEntityWithWorld packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No entity");

            ref var viewRef = ref world.GetPool<EntityViewRef<T>>().Get(entity);
            viewRef.EntityView.UpdateData();
        }

        internal void EnqueueEntityViewDestroy<T>(EcsPackedEntityWithWorld packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No entity");

            ref var viewRef = ref world.GetPool<EntityViewRef<T>>().Get(entity);
            viewRef.EntityView.Destroy();
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
            var filter = ecsWorld.Filter<ModalDialogTag>()
                .Inc<T>()
                .Exc<ModalDialogAction<T>>()
                .End(); 
            var pool = ecsWorld.GetPool<ModalDialogAction<T>>();

            foreach ( var entity in filter)
            {
                ref var action = ref pool.Add(entity);
                action.ActionIdx = idx;
            }
        }
    }

}

