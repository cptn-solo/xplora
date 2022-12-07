using Assets.Scripts.Services.App;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class ApplicationSettingsScreen : MonoBehaviour
    {
        [Inject] private AudioPlaybackService audioPlaybackService;
        [Inject] private PlayerPreferencesService playerPrefsService;

        [SerializeField] private Button closeButton;

        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Slider sfxSlider;

        public event UnityAction OnCloseButtonPressed;

        void Start()
        {
            musicToggle.isOn = audioPlaybackService.MusicToggle;
            musicSlider.value = audioPlaybackService.MusicVolume;

            sfxToggle.isOn = audioPlaybackService.SfxToggle;
            sfxSlider.value = audioPlaybackService.SfxVolume;
        }

        public void Close() => OnCloseButtonPressed?.Invoke();

        private void Awake()
        {
            musicToggle.onValueChanged.AddListener(OnMusicToggleChange);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChange);

            sfxToggle.onValueChanged.AddListener(OnSfxToggleChange);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChange);

            closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChange);
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChange);

            sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChange);
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChange);

            closeButton.onClick.RemoveListener(Close);
        }

        public void OnMusicSliderChange(float value)
        {
            if (audioPlaybackService == null)
                return;

            audioPlaybackService.MusicVolume = (int)musicSlider.value;
        }

        public void OnSfxSliderChange(float value)
        {
            if (audioPlaybackService == null)
                return;

            audioPlaybackService.SfxVolume = (int)sfxSlider.value;
        } 

        public void OnMusicToggleChange(bool value)
        {
            if (audioPlaybackService == null)
                return;

            audioPlaybackService.MusicToggle = musicToggle.isOn;
        }

        public void OnSfxToggleChange(bool value)
        {
            if (audioPlaybackService == null)
                return;

            audioPlaybackService.SfxToggle = sfxToggle.isOn;
        }
    }
}