namespace Assets.Scripts.Data
{
    public struct RelationEventItemInfo
    {
        public HeroKind Kind {  get; set; }

        public int SrcBaseValue { get; set; }
        public int SrcDiffValue { get; set; }

        public int TgtBaseValue { get; set; }
        public int TgtDiffValue { get; set; }

        public string ItemTitle { get; internal set; }
        public BarInfo SrcBarInfo { get; internal set; }
        public BarInfo TgtBarInfo { get; internal set; }
    }
}