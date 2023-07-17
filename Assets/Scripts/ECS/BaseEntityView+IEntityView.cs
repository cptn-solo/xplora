using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS
{
    public partial class BaseEntityView<T> : IEntityView<T>
        where T : struct
    {
        #region IEntityView<T>

        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public IEcsService EcsService { get; set; }
        public DataLoadDelegate<T> DataLoader { get; set; }

        public virtual void Destroy() =>
            Destroy(gameObject);

        public virtual void UpdateData() =>
            CurrentData = DataLoader?.Invoke(PackedEntity);

        #endregion

        #region IEntityView

        public void AttachChild<C>(ITransform<C> child)
            where C : struct
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            ref var transformRef = ref world.GetPool<TransformRef<C>>().Add(entity);
            transformRef.Transform = child.Transform;
        }

        public void AttachChild<C>(IItemsContainer<C> child)
            where C : struct
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

        public void Decommision()
        {
            EcsService.OnManagedSceneDecomission -= Decommision;
            OnGameObjectDestroy();
        }

        public virtual void Visualize<V>(V visualInfo, EcsPackedEntityWithWorld visualEntity) where V : struct, ISceneVisualsInfo
        {
            // base implementation is just mark visual as complete by removing it's entity from the world:
            // override if there is some animation to be played for the visualInfo
            if (visualEntity.Unpack(out var world, out var entity))
                world.DelEntity(entity);
        }
        #endregion

    }
}
