using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public class BarWithTitle : MonoBehaviour
    {
        [SerializeField] private Image barImage;
        [SerializeField] private TextMeshProUGUI titleLabel;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetInfo(BarInfo info)
        {
            var parentWidth = rectTransform.rect.width;
            barImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                info.Value * parentWidth);
            barImage.color = info.Color;

            titleLabel.text = info.Title;

        }
    }
}