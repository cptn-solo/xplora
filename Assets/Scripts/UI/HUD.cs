using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button settingsButton;
        public event UnityAction OnSettingsButtonPressed;
        private void Awake()
        {
            settingsButton.onClick.AddListener(SettingsButtonPressed);
        }

        private void SettingsButtonPressed()
        {
            OnSettingsButtonPressed?.Invoke();
        }

        private void OnDestroy()
        {
            settingsButton.onClick.RemoveListener(SettingsButtonPressed);
        }
    }
}