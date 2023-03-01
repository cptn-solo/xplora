namespace Assets.Scripts.Data
{
    public struct TeamMemberInfo
    {
        public string HeroName { get; set; }
        public int Speed { get; set; } // probably redundant if
                                       // served from instance
        public string IconName { get; internal set; }
        public string IdleSpriteName { get; internal set; }
    }
}