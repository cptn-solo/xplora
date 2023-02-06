using Assets.Scripts.UI.Common;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class UnitOverlay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI overlayText;
        [SerializeField] private BarsContainer barsContainer;

        private Transform anchor;
        private Vector3 prevAnchorScale;
        private bool destroyed;

        public void Attach(Transform anchor) =>
            this.anchor = anchor;

        internal void ResetOverlayInfo()
        {
            if (destroyed)
                return;

            overlayText.text = "";
        }
        private void OnDestroy()
        {
            destroyed = true;
            anchor = null;
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

        internal void SetOverlayText(string text)
        {
            overlayText.text = text;
        }

        internal void SetBarsInfo(
            List<BarInfo> barsInfo)
        {
            barsContainer.gameObject.SetActive(true);
            barsContainer.SetData(barsInfo);
        }

        internal void ResetBars()
        {
            if (destroyed)
                return;

            barsContainer.gameObject.SetActive(false);
        }
    }
}