using UnityEngine;

namespace Assets.Scripts
{
    public interface ITransform
    {
        public Transform Transform { get; }

        public void OnGameObjectDestroy();
    }

    public interface ITransform<T> : ITransform
    {
    }

}