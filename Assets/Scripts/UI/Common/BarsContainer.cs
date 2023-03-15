using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Common
{
    public partial class BarsContainer : MonoBehaviour
    {
        [SerializeField] private GameObject barPrefab;

        private readonly Dictionary<int, BarWithTitle> barsIndex = new();
        private BarWithTitle[] bars = new BarWithTitle[0];

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetData(BarInfo[] value) {
            if (bars.Length != value.Length)
            {
                foreach (var bar in barsIndex)
                    GameObject.Destroy(bar.Value.gameObject);

                barsIndex.Clear();
                bars = new BarWithTitle[value.Length];

                for (int i = 0; i < value.Length; i++)
                {
                    var barInfo = value[i];
                    BarWithTitle bar = SpawnBar();

                    bar.SetInfo(barInfo);

                    bars[i] = bar;
                    barsIndex.Add(barInfo.Id, bar);
                }
            }
            else
            {
                for (int i = 0; i < value.Length; i++)
                {
                    var barInfo = value[i];
                    var bar = barsIndex[barInfo.Id];
                    bar.SetInfo(barInfo);
                }
            }
        }

        public void UpdateBar(BarInfo info)
        {
            var bar = barsIndex[info.Id];
            bar.SetInfo(info);
        }

        private BarWithTitle SpawnBar()
        {
            var bar = Instantiate(barPrefab).GetComponent<BarWithTitle>();
            var barRectTransform = bar.GetComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();
            barRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 15f);
            barRectTransform.localScale = canvas.transform.localScale * transform.parent.localScale.x;
            barRectTransform.SetParent(rectTransform);
            barRectTransform.localRotation = Quaternion.identity;
            barRectTransform.anchoredPosition3D = Vector3.zero;
            barRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
            return bar;
        }

        private void OnDestroy()
        {
            OnGameObjectDestroy();
        }
    }
}