using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.Services.App
{
    public class AudioPlaybackService : MonoBehaviour
    {
        [SerializeField] private PlayerPreferencesService playerPreferencesService;
        [SerializeField] private AudioMixer mixer;

        private const string fxGroupKey = "FXVolume";
        private const string musicGroupKey = "MusicVolume";

        private void Start()
        {
            InitAudioPlayback();            
        }

        private void SetMixerValue(string key, float normalized)
        {
            //mixer.SetFloat(key, Mathf.Log10(normalized) * 50);
            mixer.SetFloat(key, normalized); // sliders set to -80:+20 (whole numbers)
        }

        public bool SfxToggle
        {
            get => PlayerPrefs.GetInt(PlayerPreferencesService.SfxToggleKey) != 0;
            set
            {
                PlayerPrefs.SetInt(PlayerPreferencesService.SfxToggleKey, value ? 1 : 0);
                SetMixerValue(fxGroupKey, value ? SfxVolume : -80);
            }
        }

        public int SfxVolume { 
            get => SfxToggle ? PlayerPrefs.GetInt(PlayerPreferencesService.SfxVolumeKey) : -80;
            set {
                PlayerPrefs.SetInt(PlayerPreferencesService.SfxVolumeKey, value);
                SetMixerValue(fxGroupKey, value);
                if (mixer.GetFloat(fxGroupKey, out var current))
                    Debug.LogFormat("Set SfxVolume {0}", current);
            }
        }

        public bool MusicToggle {
            get => PlayerPrefs.GetInt(PlayerPreferencesService.MusicToggleKey) != 0;
            set {
                PlayerPrefs.SetInt(PlayerPreferencesService.MusicToggleKey, value ? 1 : 0);
                SetMixerValue(musicGroupKey, value ? MusicVolume : -80);
            }
        }

        public int MusicVolume { 
            get => MusicToggle ? PlayerPrefs.GetInt(PlayerPreferencesService.MusicVolumeKey) : -80;
            set {
                PlayerPrefs.SetInt(PlayerPreferencesService.MusicVolumeKey, value);
                SetMixerValue(musicGroupKey, value);
                //AudioListener.volume = value;
                if (mixer.GetFloat(musicGroupKey, out var current))
                    Debug.LogFormat("Set MusicVolume {0}", current);
            }
        }

        public void InitAudioPlayback()
        {
            //AudioListener.volume = MusicVolume;
            SetMixerValue(musicGroupKey, MusicVolume);
            SetMixerValue(fxGroupKey, SfxVolume);
        }
    }
}
