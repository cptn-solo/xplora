using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public class BaseEntityView<T> : MonoBehaviour, IEntityView<T>
    where T : struct
    {
        public virtual T? CurrentData { get; private set; }

        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public DataLoadDelegate<T> DataLoader { get; set; }

        public Transform Transform => transform;

        public void AttachChild<C>(ITransform<C> child)
            where C : struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            ref var transformRef = ref world.GetPool<TransformRef<C>>().Add(entity);
            transformRef.Transform = child.Transform;
        }

        public void AttachChild<C>(IItemsContainer<C> child)
            where C: struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            ref var transformRef = ref world.GetPool<ItemsContainerRef<C>>().Add(entity);
            transformRef.Container = child;
        }

        public void AttachChild<C>(IDataView<C> child)
            where C : struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            ref var transformRef = ref world.GetPool<DataViewRef<C>>().Add(entity);
            transformRef.DataView = child;
            child.Reset();
        }

        public void DetachChild<C>(IItemsContainer<C> child)
            where C : struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            var pool = world.GetPool<ItemsContainerRef<C>>();
            if (!pool.Has(entity))
                return;

            ref var transformRef = ref pool.Get(entity);
            transformRef.Container = null;
            pool.Del(entity);
        }

        public void DetachChild<C>(ITransform<C> child)
            where C : struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            var pool = world.GetPool<TransformRef<C>>();
            if (!pool.Has(entity))
                return;

            ref var transformRef = ref pool.Get(entity);
            transformRef.Transform = null;
            pool.Del(entity);
        }

        public void DetachChild<C>(IDataView<C> child)
            where C : struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            var pool = world.GetPool<DataViewRef<C>>();
            if (!pool.Has(entity))
                return;

            ref var transformRef = ref pool.Get(entity);
            transformRef.DataView = null;
            pool.Del(entity);
        }

        public virtual void Destroy()
        {
            GameObject.Destroy(gameObject);
        }

        public void OnGameObjectDestroy()
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            var pool = world.GetPool<EntityViewRef<T>>();
            if (!pool.Has(entity))
                return;

            ref var entityViewRef = ref pool.Get(entity);
            entityViewRef.EntityView = null;
            pool.Del(entity);
        }

        public virtual void UpdateData()
        {
            CurrentData = DataLoader?.Invoke(PackedEntity);
        }
    }
}
