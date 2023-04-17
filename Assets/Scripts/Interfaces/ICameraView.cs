using UnityEngine;

namespace Assets.Scripts
{
    public interface ICameraView {
        public Vector3 GetViewPosition(Vector3 worldPosition);
    }

}