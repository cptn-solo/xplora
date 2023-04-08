using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.ECS
{
    public class BaseEntityViewContainer<S, T> : MonoBehaviour, ITransform<T>
        where S : IEcsService where T : struct
    {
        public Transform Transform => transform;

        private IEcsService ecsService;

        [Inject]
        public void Construct(S ecsService)
        {
            ecsService.RegisterTransformRef<T>(this);
            this.ecsService = ecsService;
        }

        private void OnDestroy()
        {
            OnGameObjectDestroy();
        }

        public void OnGameObjectDestroy()
        {
            ecsService.UnregisterTransformRef<T>(this);
        }
    }
}
