namespace Assets.Scripts.UI.Data
{
    public enum CommonSoundEvent
    {
        NA = 0,
        HeroLibrary = 100,        
        Battle = 200,
        BattleWon = 300,
        BattleLost = 400,
    }

    public static class SoundUtils
    {
        public static string FileForCSE(CommonSoundEvent evt)
            => evt switch
            {
                CommonSoundEvent.HeroLibrary => "hero_choice",
                CommonSoundEvent.Battle => "battle",
                CommonSoundEvent.BattleWon => "win2",
                CommonSoundEvent.BattleLost => "defeat",
                _ => ""
            };
    }
}
