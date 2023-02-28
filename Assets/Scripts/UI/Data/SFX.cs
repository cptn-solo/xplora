namespace Assets.Scripts.UI.Data
{
    public struct SFX
    {
        public string FileName { get; set; }
        public bool Loop { get;  set; }
        public bool IsMusic { get; set; }
        public float VolumeScale { get; set; }

        #region Sounds

        public static SFX Named(string name, float volume = 1f)
        {
            SFX ret = default;
            ret.FileName = name;
            ret.VolumeScale = volume;
            return ret;
        }

        #endregion

        #region Music

        public static SFX MainTheme =>
            CommonSoundEvent.Battle.ThemeForEvent();
        public static SFX LibraryTheme =>
            CommonSoundEvent.HeroLibrary.ThemeForEvent();
        public static SFX WinTheme =>
            CommonSoundEvent.BattleWon.ThemeForEvent();
        public static SFX LooseTheme =>
            CommonSoundEvent.BattleLost.ThemeForEvent();
        public static SFX RaidTheme =>
            CommonSoundEvent.Raid.ThemeForEvent();

        #endregion
    }
}
