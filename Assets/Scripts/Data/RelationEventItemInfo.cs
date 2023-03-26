namespace Assets.Scripts.Data
{
    public struct RelationEventItemInfo
    {
        public string ItemTitle { get; internal set; }
        public BarInfo SrcBarInfo { get; internal set; }
        public BarInfo TgtBarInfo { get; internal set; }
    }
}