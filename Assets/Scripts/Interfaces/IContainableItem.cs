using UnityEngine;

namespace Assets.Scripts
{
    public interface IContainableItem<T> where T : struct
    {
        public void SetInfo(T info);
        
        /// <summary>
        /// Will move the MovableCard from the sourceTransform position to the center of the container
        /// </summary>
        /// <param name="info"></param>
        /// <param name="sourceTransform"></param>
        public void SetInfoAnimated(T info, Transform sourceTransform);
        public Transform MovableCard { get; }
    }

}