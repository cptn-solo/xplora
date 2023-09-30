namespace Assets.Scripts.Data
{
    public struct HeroKindBarInfo : IContainableItemInfo<int>
    {
     
        public int Id { 
            readonly get => (int)Kind; 
            set => Kind = (HeroKind)value; 
        }
        
        public HeroKind Kind { get; set; }

        public int CurrentValue { get; set; }
        public int DiffValue { get; set; }

        public string ItemTitle { get; internal set; }
        public BarInfo BarInfo { get; internal set; }
    }
}