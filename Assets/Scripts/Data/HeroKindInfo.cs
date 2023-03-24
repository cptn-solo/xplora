namespace Assets.Scripts.Data
{
    public struct HeroKindInfo
    {
        public HeroKind Kind { get; internal set; }
        public int Level { get; internal set; }

        public static HeroKindInfo Zero(HeroKind kind)
        {
            return new HeroKindInfo() { Kind = kind, Level = 0 };
        }
    }
}