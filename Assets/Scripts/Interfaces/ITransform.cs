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

    public interface IItemsContainer<T> : ITransform
    {
        public void SetItemInfo(T info);
        public void SetItems(T[] items);
        public void RemoveItem(T info);
        public void Reset();
    }

}