using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public class BarWithTitle : BaseContainableItem<BarInfo>
    {
        [SerializeField] private Image barImage;
        [SerializeField] private Image deltaImage;
        [SerializeField] private TextMeshProUGUI titleLabel;

        protected override void ApplyInfoValues(BarInfo info)
        {
            titleLabel.text = info.Title;

            barImage.color = info.Color;

            var parentWidth = rectTransform.rect.width;
            var parentHeight = rectTransform.rect.height;
            
            var pivot = barImage.rectTransform.pivot;

            if (pivot.y == 0 || pivot.y == 1)
            {
                var barHeight = info.Value * parentHeight;
                barImage.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical, barHeight);            
            }
            else
            {
                var barWidth = info.Value * parentWidth;
                barImage.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal, barWidth);
            }

            if (info.Delta == 0f)
            {
                deltaImage.enabled = false;
            }
            else
            {
                deltaImage.enabled = true;                

                var deltaPos = deltaImage.rectTransform.anchoredPosition;
                var deltaPivot = deltaImage.rectTransform.pivot;

                if (deltaPivot.y == 0f || deltaPivot.y == 1f)
                {
                    var deltaHeight = info.Delta * parentHeight;
                    deltaImage.rectTransform.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Vertical, deltaHeight);

                    if (deltaPivot.y == 1f)
                        deltaPos.Set(deltaPos.x, barImage.rectTransform.rect.yMin);
                    else if (deltaPivot.y == 0f)
                        deltaPos.Set(deltaPos.x, barImage.rectTransform.rect.yMax);
                }                
                else
                {
                    var deltaWidth = info.Delta * parentWidth;
                    deltaImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    deltaWidth);

                    if (deltaPivot.x == 1f)
                        deltaPos.Set(barImage.rectTransform.rect.xMin, deltaPos.y);
                    else if (deltaPivot.x == 0f)
                        deltaPos.Set(barImage.rectTransform.rect.xMax,  deltaPos.y);
                }

                deltaImage.rectTransform.anchoredPosition = deltaPos;
                
                deltaImage.color = info.DeltaColor;

            }            

        }
    }
 }