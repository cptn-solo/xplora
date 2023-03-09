using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    public enum CommonSoundEvent
    {
        NA = 0,
        HeroLibrary = 100,        
        Battle = 200,
        BattleWon = 300,
        BattleLost = 400,
        Raid = 500,
        FieldCellHover = 600,
        FootSteps = 700,
        StaminaSource = 800,
        HPSource = 900,
    }

    public static class SoundUtils
    {
        public static string[] RaidFiles = new string[3] {
            "The_Lost_Caravan",
            "Bonus_Track",
            "homm2_21"
        };
        public static string FileForCSE(this CommonSoundEvent evt)
            => evt switch
            {
                CommonSoundEvent.HeroLibrary => "hero_choice",
                CommonSoundEvent.Battle => "battle",
                CommonSoundEvent.BattleWon => "win2",
                CommonSoundEvent.BattleLost => "defeat",
                CommonSoundEvent.Raid => RaidFiles[Random.Range(0, RaidFiles.Length)],
                CommonSoundEvent.FieldCellHover => "cupclick",
                CommonSoundEvent.FootSteps => "run",
                CommonSoundEvent.StaminaSource => "stamina_source",
                CommonSoundEvent.HPSource => "hp_source",
                _ => ""
            };

        public static float VolumeForCSE(this CommonSoundEvent evt)
            => evt switch
            {
                CommonSoundEvent.FootSteps => .1f,
                _ => 1f
            };

        public static SFX ThemeForEvent(this CommonSoundEvent evt, float volume = 1f)
        {
            SFX ret = default;
            ret.FileName = evt.FileForCSE();
            ret.VolumeScale = volume;
            ret.IsMusic = true;
            ret.Loop = true;
            return ret;
        }

        public static SFX SoundForEvent(this CommonSoundEvent evt, float volume = 1f)
        {
            SFX ret = default;
            ret.FileName = evt.FileForCSE();
            ret.VolumeScale = evt.VolumeForCSE();
            return ret;
        }


    }
}
