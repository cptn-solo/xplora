using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.ECS
{
    public class CameraEntityView<S> : BaseEntityView<CameraTag>, ICameraView
        where S : IEcsService
    {
        protected Camera activeCamera;

        [Inject]
        public void Construct(S service) =>
            service.RegisterEntityView(this);

        protected override void OnBeforeAwake() =>
            activeCamera = GetComponent<Camera>();

        /// <summary>
        /// Gets relative (0-1) position vector of a world position in
        /// screen space
        /// </summary>
        /// <param name="pos">world position</param>
        /// <returns>if result.x > 0.5F then target is on the right side (and so on)
        /// </returns>
        public Vector3 GetViewPosition(Vector3 pos) =>
            activeCamera.WorldToViewportPoint(pos);
    }

}
