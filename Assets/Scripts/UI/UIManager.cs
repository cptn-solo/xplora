using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Assets.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {        
        private HUD HUD;
        private ApplicationSettingsScreen appSettingsScreen;
        private bool pauseBetweenScenes;
        
        [Inject] private readonly MenuNavigationService nav;

        [Inject]
        public void Construct(MenuNavigationService nav)
        {
            nav.UIManager = this;
        }

        private void Awake() =>
            DontDestroyOnLoad(gameObject);

        private void Start()
        {
            HUD = GetComponentInChildren<HUD>(true);
            HUD.OnSettingsButtonPressed += HUD_OnSettingsButtonPressed;
            HUD.OnMenuButtonPressed += HUD_OnMenuButtonPressed;
            
            appSettingsScreen = GetComponentInChildren<ApplicationSettingsScreen>(true);
            appSettingsScreen.OnCloseButtonPressed += AppSettingsScreen_OnCloseButtonPressed;
            
            //ToggleScreen(Screens.Hub);
            //ToggleScreen(Screens.Battle);
            //ToggleScreen(Screens.HeroesLibrary);
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
        private void HUD_OnMenuButtonPressed()
        {
            nav.NavigateToScreen(Screens.Hub);
        }

        public void ToggleScreen(Screens screen, NavigationCallback callback)
        {
            var sceneName = screen switch
            {
                Screens.HeroesLibrary => "Library",
                Screens.Battle => "Battle",
                Screens.Raid => "World",
                _ => "Lobby"
            };

            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)
                .completed += (op) => {
                    Debug.Log($"Loaded scene: {sceneName}");
                    callback(screen);
                };
        }

        private IEnumerator LoadLobbyScene()
        {
            var op = SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < .9f)
                yield return null;

            if (pauseBetweenScenes)
                yield return new WaitForSecondsRealtime(3.0f);

            op.allowSceneActivation = true;

            while (!op.isDone)
                yield return null;

            Debug.Log("Lobby scene loaded");
        }
    }
}