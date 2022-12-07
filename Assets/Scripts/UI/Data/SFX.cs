namespace Assets.Scripts.UI.Data
{
    public struct SFX
    {
        public string FileName { get; private set; }
        public bool Loop { get; private set; }
        public bool IsMusic { get; private set; }

        #region Sounds

        public static SFX Named(string name)
        {
            SFX ret = default;
            ret.FileName = name;
            return ret;
        }

        public static SFX MeleeAttack
        {
            get
            {
                SFX ret = default;
                ret.FileName = "melee_att";
                return ret;
            }
        }

        #endregion

        #region Music
        public static SFX MainTheme
        {
            get
            {
                SFX ret = default;
                ret.FileName = "battle";
                ret.IsMusic = true;
                ret.Loop = true;
                return ret;
            }
        }
        #endregion
    }
}
