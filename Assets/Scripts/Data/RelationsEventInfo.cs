namespace Assets.Scripts.Data
{
    public struct RelationsEventInfo
    {
        public string EventTitle { get; internal set; }
        public string SrcIconName { get; internal set; }
        public string TgtIconName { get; internal set; }
        public string EventText { get; internal set; }

        public RelationScoreInfo ScoreInfo { get; internal set; }
        public RelationEventItemInfo[] EventItems { get; internal set; }

        public string[] ActionTitles { get; internal set; }
    }
}