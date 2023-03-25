using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public class BarWithTitle : MonoBehaviour
    {
        [SerializeField] private Image barImage;
        [SerializeField] private Image deltaImage;
        [SerializeField] private TextMeshProUGUI titleLabel;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetInfo(BarInfo info)
        {
            titleLabel.text = info.Title;

            var parentWidth = rectTransform.rect.width;
            barImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                info.Value * parentWidth);
            barImage.color = info.Color;

            if (info.Delta == 0f)
            {
                deltaImage.enabled = false;
            }
            else
            {
                deltaImage.enabled = true;                
                var deltaPos = deltaImage.rectTransform.anchoredPosition;
                var pivot = deltaImage.rectTransform.pivot;
                var deltaWidth = info.Delta * parentWidth;
                deltaImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                deltaWidth);

                if (pivot.x == 1f)
                    deltaPos.Set(barImage.rectTransform.rect.xMin, deltaPos.y);
                else if (pivot.x == 0f)
                    deltaPos.Set(barImage.rectTransform.rect.xMax,  deltaPos.y);

                deltaImage.rectTransform.anchoredPosition = deltaPos;
                
                deltaImage.color = info.DeltaColor;

            }            
        }
    }
}