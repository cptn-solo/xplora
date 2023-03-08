using UnityEngine;
using Leopotam.EcsLite;
using Assets.Scripts.ECS.Data;
using System;

namespace Assets.Scripts.Services
{
    public partial class BaseEcsService : MonoBehaviour
    {
        protected EcsWorld ecsWorld { get; set; }

        protected IEcsSystems ecsRunSystems { get; set; }
        protected IEcsSystems ecsInitSystems { get; set; }

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
            where T: struct
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



    }

}

