namespace Assets.Scripts.Data
{
    public struct RelationBonusInfo
    {
        public int Bonus { get; set; }
        public HeroKind[] TargetKinds { get; set; }

        public static RelationBonusInfo Empty
        {
            get =>
            new()
            { Bonus = 0, TargetKinds = new HeroKind[0] };
        }
    }
}