namespace Assets.Scripts.Data
{
    public struct WorldEventInfo
    {
        public string IconName { get; internal set; } // with path
        public string EventTitle { get; internal set; }
        public string EventText { get; internal set; }

        public string[] ActionTitles { get; internal set; }
    }
}