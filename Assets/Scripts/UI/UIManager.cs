using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Device;
using Zenject;

namespace Assets.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        
        private GameObject activeScreen;
        private MenuScreen[] screens;

        private HUD HUD;
        private ApplicationSettingsScreen appSettingsScreen;

        [Inject]
        public void Construct(MenuNavigationService nav) =>
            nav.UIManager = this;

        private void Awake() =>
            DontDestroyOnLoad(this.gameObject);

        private void Start()
        {
            HUD = GetComponentInChildren<HUD>(true);
            HUD.OnSettingsButtonPressed += HUD_OnSettingsButtonPressed;
            
            appSettingsScreen = GetComponentInChildren<ApplicationSettingsScreen>(true);
            appSettingsScreen.OnCloseButtonPressed += AppSettingsScreen_OnCloseButtonPressed;

            screens = GetComponentsInChildren<MenuScreen>(true);

            foreach (var screen in screens)
                screen.gameObject.SetActive(false);

            //ToggleScreen(Screens.Hub);
            //ToggleScreen(Screens.Battle);
            ToggleScreen(Screens.HeroesLibrary);

        }

        private void AppSettingsScreen_OnCloseButtonPressed()
        {
            appSettingsScreen.gameObject.SetActive(false);
            HUD.gameObject.SetActive(true);
        }

        private void HUD_OnSettingsButtonPressed()
        {
            appSettingsScreen.gameObject.SetActive(true);
            HUD.gameObject.SetActive(false);
        }

        public void ToggleScreen(Screens screen)
        {
            var obj = screens.Where(x => x.Screen == screen).FirstOrDefault();
            
            if (obj == default)
                return;

            if (activeScreen != null)
                activeScreen.SetActive(false);

            obj.gameObject.SetActive(true);
            this.activeScreen = obj.gameObject;

        }

    }
}