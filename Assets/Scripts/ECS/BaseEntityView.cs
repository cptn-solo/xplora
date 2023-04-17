using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public partial class BaseEntityView<T> : MonoBehaviour
        where T : struct
    {
        public virtual T? CurrentData { get; private set; }

        protected virtual void OnBeforeStart()
        {
        }

        protected virtual void OnBeforeAwake()
        {
        }

        protected virtual void OnBeforeDestroy()
        {
        }

        private void Start()
        {
            OnBeforeStart();

            foreach (var child in GetComponentsInChildren<IEntityViewChild>(true))
                child.AttachToEntityView();
        }

        private void Awake()
        {
            OnBeforeAwake();
        }

        private void OnDestroy()
        {
            OnBeforeDestroy();

            foreach (var child in GetComponentsInChildren<IEntityViewChild>())
                child.DetachFromEntityView();

            OnGameObjectDestroy();
        }

    }
}
