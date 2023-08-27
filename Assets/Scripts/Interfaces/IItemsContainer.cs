using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IItemsContainer<T> : ITransform
    {
        public void SetItemInfo(T info);
        public void SetInfo(T[] value);
        public void RemoveItem(T info);
        public void Reset();

        /// <summary>
        /// Spawns or gets an item and moves it from the posion of sourceTransform
        /// to the zero position of the container
        /// </summary>
        /// <param name="info">Item info for spawner</param>
        /// <param name="sourceTransform">Transform to take start position from</param>
        public void SetItemInfoAnimatedMove(T info, Transform sourceTransform);
    }

}