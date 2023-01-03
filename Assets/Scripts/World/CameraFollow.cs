using UnityEngine;

namespace Assets.Scripts.World
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform target;
        
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float distance = 5f;
        [SerializeField] private float angle = 30f;
        public void Attach(Transform target)
        {
            this.target = target;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var pos = target.position - cameraTransform.forward * distance;
            cameraTransform.position = pos;
        }
    }
}