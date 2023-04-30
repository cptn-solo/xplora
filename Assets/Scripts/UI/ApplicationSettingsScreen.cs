using Assets.Scripts.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class ApplicationSettingsScreen : MonoBehaviour
    {
        private PlayerInputActions inputActions;

        [Inject] private AudioPlaybackService audioPlaybackService = default;
        [Inject] private PlayerPreferencesService playerPrefsService = default;

        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle disableRngToggle;

        [SerializeField] private Button closeButton;

        private HandlePanelVisibility panel;

        public event UnityAction OnCloseButtonPressed;

        public void Close() => OnCloseButtonPressed?.Invoke();

        void Start()
        {
            musicToggle.isOn = audioPlaybackService.MusicToggle;
            musicSlider.value = audioPlaybackService.MusicVolume;

            sfxToggle.isOn = audioPlaybackService.SfxToggle;
            sfxSlider.value = audioPlaybackService.SfxVolume;

            disableRngToggle.isOn = playerPrefsService.DisableRNGToggle;
        }

        private void Awake()
        {
            musicToggle.onValueChanged.AddListener(OnMusicToggleChange);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChange);

            sfxToggle.onValueChanged.AddListener(OnSfxToggleChange);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChange);

            disableRngToggle.onValueChanged.AddListener(OnDisableRngToggleChange);

            closeButton.onClick.AddListener(Close);

            panel = GetComponentInChildren<HandlePanelVisibility>();

            panel.OnClickedOutside += Close;

            inputActions = new();
        }
        private void OnEnable()
        {
            inputActions.UI.Cancel.performed += Cancel_performed;
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.UI.Cancel.performed -= Cancel_performed;
            inputActions.Disable();
        }

        private void Cancel_performed(InputAction.CallbackContext obj)
        {
            Close();
        }

        private void OnDestroy()
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChange);
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChange);

            sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChange);
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChange);
            
            disableRngToggle.onValueChanged.RemoveListener(OnDisableRngToggleChange);

            closeButton.onClick.RemoveListener(Close);
            panel.OnClickedOutside -= Close;
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
        public void OnDisableRngToggleChange(bool value)
        {
            if (playerPrefsService == null)
                return;

            playerPrefsService.DisableRNGToggle = disableRngToggle.isOn;
        }
    }
}