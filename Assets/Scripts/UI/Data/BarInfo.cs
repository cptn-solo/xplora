using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public struct BarInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Color Color { get; set; }
        public float Value { get; set; } // 0 - 1

        public static BarInfo EmptyBarInfo(int id, string text, Color color, float value)
        {
            BarInfo barInfo = default;
            barInfo.Id = id;
            barInfo.Title = text;
            barInfo.Color = color;
            barInfo.Value = value;
            return barInfo;
        }

        public static BarInfo Default => EmptyBarInfo(-1, "", Color.green, 1.0f);
    }
}