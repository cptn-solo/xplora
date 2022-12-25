namespace Assets.Scripts.UI.Data
{
    public struct RoundSlotInfo
    {
        public int HeroId { get; private set; }
        public string HeroName { get; private set; }
        public int TeamId { get; private set; }

        public static RoundSlotInfo Create(Hero hero)
        {
            RoundSlotInfo info = default;
            info.HeroId = hero.Id;
            info.TeamId = hero.TeamId;
            info.HeroName = hero.Name;
            return info;
        }
    }
}