namespace Assets.Scripts.Data
{
    public struct RelationTargetInfo
    {
        public int Rate { get; set; }
        public int Compared { get; set; } // -1 less, 0 equals, 1 above
        public RelationBonusInfo IncomingBonus { get; set; }
        public RelationBonusInfo OutgoingBonus { get; set; }
    }
}