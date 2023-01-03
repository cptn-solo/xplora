using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button menuButton;
        
        public event UnityAction OnSettingsButtonPressed;
        public event UnityAction OnMenuButtonPressed;

        private void Awake()
        {
            settingsButton.onClick.AddListener(SettingsButtonPressed);
            menuButton.onClick.AddListener(MenuButtonPressed);
        }

        private void SettingsButtonPressed()
        {
            OnSettingsButtonPressed?.Invoke();
        }
        private void MenuButtonPressed()
        {
            OnMenuButtonPressed?.Invoke();
        }

        private void OnDestroy()
        {
            settingsButton.onClick.RemoveListener(SettingsButtonPressed);
            menuButton.onClick.RemoveListener(MenuButtonPressed);
        }
    }
}