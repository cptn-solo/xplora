using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerPreferencesService : MonoBehaviour
    {
        public const string MusicToggleKey = "MusicToggle";
        public const string SfxToggleKey = "SfxToggle";
        public const string MusicVolumeKey = "MusicVolume";
        public const int MusicVolumeDefault = -20;

        public const string SfxVolumeKey = "SfxVolume";
        public const int SfxVolumeDefault = 0;

        public const string ScoreKey = "Score"; // temporary, to keep score between games
        public const string BalanceKey = "Balance"; // temporary, to keep score between games        
        public const string WeaponKey = "Weapon"; // temporary, to keep score between games        
        public const string PerkKey = "Perk"; // temporary, to keep score between games        

        public event Action<float> OnCameraSencitivityChanged;
        public event Action<bool> OnCameraControlChanged;

        private void Awake()
        {
            InitPlayerPreferences();
#if UNITY_EDITOR
            InitDeveloperPreferences();
#endif
        }

        public void InitPlayerPreferences()
        {
            if (!PlayerPrefs.HasKey(MusicToggleKey))
                PlayerPrefs.SetInt(MusicToggleKey, 1);
            
            if (!PlayerPrefs.HasKey(SfxToggleKey))
                PlayerPrefs.SetInt(SfxToggleKey, 1);

            if (!PlayerPrefs.HasKey(MusicVolumeKey))
                PlayerPrefs.SetInt(MusicVolumeKey, MusicVolumeDefault);

            if (!PlayerPrefs.HasKey(SfxVolumeKey))
                PlayerPrefs.SetInt(SfxVolumeKey, SfxVolumeDefault);
            
        }

        public void InitDeveloperPreferences()
        {
            
        }
    }
}
