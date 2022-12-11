namespace Assets.Scripts.UI.Data
{
    public struct SFX
    {
        public string FileName { get; private set; }
        public bool Loop { get; private set; }
        public bool IsMusic { get; private set; }

        #region Sounds

        public static SFX Enumed(CommonSoundEvent evt)
        {
            SFX ret = default;
            ret.FileName = SoundUtils.FileForCSE(evt);
            return ret;
        }

        public static SFX Named(string name)
        {
            SFX ret = default;
            ret.FileName = name;
            return ret;
        }

        #endregion

        #region Music

        public static SFX MainTheme =>
            ThemeForEvent(CommonSoundEvent.Battle);
        public static SFX LibraryTheme =>
            ThemeForEvent(CommonSoundEvent.HeroLibrary);
        public static SFX WinTheme =>
            ThemeForEvent(CommonSoundEvent.BattleWon);
        public static SFX LooseTheme =>
            ThemeForEvent(CommonSoundEvent.BattleLost);

        public static SFX ThemeForEvent(CommonSoundEvent evt)
        {
            SFX ret = default;
            ret.FileName = SoundUtils.FileForCSE(evt);
            ret.IsMusic = true;
            ret.Loop = true;
            return ret;
        }
        #endregion
    }
}
