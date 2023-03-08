using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class UnitOverlay : BaseEntityView<BarsInfo>
    {
        private Transform anchor;
        private Vector3 prevAnchorScale;

        public void Attach(Transform anchor) =>
            this.anchor = anchor;

        private void OnDestroy()
        {
            anchor = null;
            OnGameObjectDestroy();
        }

        private void Update()
        {
            if (anchor == null)
                return;

            var pos = anchor.transform.position;
            var ray = Camera.main.ScreenPointToRay(pos);
            var overlayPos = pos + (ray.direction * -1) * 10f;
            transform.position = overlayPos;


            if (prevAnchorScale != Vector3.zero &&
                prevAnchorScale.x != anchor.transform.localScale.x)
                transform.localScale *= anchor.transform.localScale.x / prevAnchorScale.x;

            prevAnchorScale = anchor.localScale;
        }
    }
}