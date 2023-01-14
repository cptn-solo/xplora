using Assets.Scripts.UI.Data;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Assets.Scripts.Services.App
{
    public class AudioPlaybackService : MonoBehaviour
    {
        [SerializeField] private PlayerPreferencesService playerPreferencesService;
        [SerializeField] private AudioMixer mixer;

        [SerializeField] private GameObject musicPrefab;
        [SerializeField] private GameObject soundsPrefab;

        private IngameMusic music;
        private IngameSounds sounds;

        private SFX currentTheme;

        public SFX CurrentTheme => currentTheme;

        private const string fxGroupKey = "FXVolume";
        private const string musicGroupKey = "MusicVolume";

        public void Play(SFX sfx)
        {
            if (sfx.IsMusic)
            {
                currentTheme = sfx;
                music.Play(sfx);
            }
            else
            {
                sounds.SoundEventHandler(sfx);
            }
        }
        public void Stop()
        {
            if (music != null)
                music.Stop();
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
            music = Instantiate(musicPrefab).GetComponent<IngameMusic>();
            DontDestroyOnLoad(music);

            sounds = Instantiate(soundsPrefab).GetComponent<IngameSounds>();
            DontDestroyOnLoad(sounds);

            //AudioListener.volume = MusicVolume;
            SetMixerValue(musicGroupKey, MusicVolume);
            SetMixerValue(fxGroupKey, SfxVolume);
        }

        internal void AttachServices(MenuNavigationService menuNavigationService)
        {
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
            menuNavigationService.OnNavigationToScreenComplete += MenuNavigationService_OnNavigationToScreenComplete;
        }

        private void MenuNavigationService_OnNavigationToScreenComplete(Screens arg0)
        {
            switch (arg0)
            {
                case Screens.NA:
                    break;
                case Screens.Hub:
                    break;
                case Screens.Raid:
                    break;
                case Screens.Missions:
                    break;
                case Screens.Resources:
                    break;
                case Screens.Heroes:
                    break;
                case Screens.Buildings:
                    break;
                case Screens.Battle:
                    break;
                case Screens.HeroesLibrary:
                    Play(SFX.LibraryTheme);
                    break;
                default:
                    break;
            }

        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(Screens arg0)
        {
            Stop();
        }
    }
}
