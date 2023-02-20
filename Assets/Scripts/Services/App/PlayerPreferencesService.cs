using UnityEngine;

namespace Assets.Scripts.Services
{
    public class PlayerPreferencesService : MonoBehaviour
    {
        public const string MusicToggleKey = "MusicToggle";
        public const string SfxToggleKey = "SfxToggle";
        public const string MusicVolumeKey = "MusicVolume";
        public const int MusicVolumeDefault = -20;

        public const string SfxVolumeKey = "SfxVolume";
        public const int SfxVolumeDefault = 0;

        public const string DisableRNGForSpecs = "DisableRNGForSpecs";

        public bool DisableRNGToggle {
            get => PlayerPrefs.GetInt(DisableRNGForSpecs) != 0;
            set => PlayerPrefs.SetInt(DisableRNGForSpecs, value ? 1 : 0);
        }

        private void Awake()
        {
            InitPlayerPreferences();
#if UNITY_EDITOR
            InitDeveloperPreferences();
#endif
        }

        public void InitPlayerPreferences()
        {
            if (!PlayerPrefs.HasKey(DisableRNGForSpecs))
                PlayerPrefs.SetInt(DisableRNGForSpecs, 0);

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
