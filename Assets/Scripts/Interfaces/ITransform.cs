using UnityEngine;

namespace Assets.Scripts
{    
    public interface ITransform
    {
        public Transform Transform { get; }

        public void OnGameObjectDestroy();
    }

    public interface IEntityViewChild
    {
        public void AttachToEntityView();
        public void DetachFromEntityView();
    }

    public interface ITransform<T> : ITransform
    {
    }

    public interface IDataView<T> : ITransform
    {
        public void SetInfo(T info);
        public void Reset();
    }

    public interface IItemsContainer<T> : ITransform
    {
        public void SetItemInfo(T info);
        public void SetItems(T[] items);
        public void RemoveItem(T info);
        public void Reset();
    }

    public interface ICameraView {
        public Vector3 GetViewPosition(Vector3 worldPosition);
    }

}