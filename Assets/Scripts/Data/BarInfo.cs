using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct BarInfo : IContainableItemInfo<int>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public float Value { get; set; } // 0 - 1
        public Color Color { get; set; }
        public float Delta { get; set; } // 0 - 1 but supposed to be less then 1
        public Color DeltaColor { get; set; }

        public static BarInfo EmptyBarInfo(int id, string text, 
            Color? color = null, float value = 0, 
            Color? deltaColor = null, float deltaValue = 0)
        {
            BarInfo barInfo = default;
            barInfo.Id = id;
            barInfo.Title = text;

            if (color == null)
            {
                var c = new Color
                {
                    a = 0f
                };

                color = c;
            }
            barInfo.Color = (Color)color;
            barInfo.Value = value;

            if (deltaColor == null)
            {
                var c = new Color
                {
                    a = 0f
                };

                deltaColor = c;
            }
            barInfo.DeltaColor = (Color)deltaColor;
            barInfo.Delta = deltaValue;

            return barInfo;
        }

        public static BarInfo Default => EmptyBarInfo(-1, "", Color.green, 1.0f);
    }
}