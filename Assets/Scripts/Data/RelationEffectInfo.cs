using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct RelationEffectInfo : IContainableItemInfo<int>
    {
        public int Id { get; set; }
     
        public string HeroIcon { get; internal set; }
        
        public BundleIcon EffectIcon { get; internal set; }
        public Color EffectIconColor { get; internal set; }
     
        public string EffectText { get; internal set; }
    }
}