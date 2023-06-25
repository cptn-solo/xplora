using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct BagedIconInfo : IContainableItemInfo<int>
    {        
        public int Id { get; set; }
        public BundleIcon Icon;
        public Color IconColor;
        public Color? BackgroundColor;
        public string BadgeText;
    }


}