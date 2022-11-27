using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{

    public class BarsContainer : MonoBehaviour
    {
        [SerializeField] private GameObject barPrefab;

        private readonly Dictionary<int, BarWithTitle> barsIndex = new();

        private readonly List<BarWithTitle> bars = new();
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        public void SetData(List<BarInfo> value) {
            var canvas = GetComponentInParent<Canvas>();
            if (bars.Count == 0)
            {
                var arr = new BarWithTitle[value.Count];                
                for (int i = 0; i < value.Count; i++)
                {
                    var barInfo = value[i];
                    var bar = Instantiate(barPrefab).GetComponent<BarWithTitle>();
                    var barRectTransform = bar.GetComponent<RectTransform>();
                    barRectTransform.localScale = canvas.transform.localScale;
                    barRectTransform.SetParent(rectTransform);
                    barRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
                    
                    bar.SetInfo(barInfo);

                    arr[i] = bar;
                    barsIndex[barInfo.Id] = bar;
                }
                bars.AddRange(arr);
            }
            else
            {
                for (int i = 0; i < value.Count; i++)
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

    }
}