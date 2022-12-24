using Assets.Scripts.UI;
using Assets.Scripts.UI.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public delegate void NavigationCallback(Screens screen);

    public class MenuNavigationService : MonoBehaviour
    {
        public UIManager UIManager { get; set; }
        public event UnityAction<Screens> OnBeforeNavigateToScreen;
        public event UnityAction<Screens> OnNavigationToScreenComplete;

        public void NavigateToScreen(Screens screen)
        {
            Debug.Log(screen);

            if (UIManager == null)
                return;

            OnBeforeNavigateToScreen?.Invoke(screen);

            UIManager.ToggleScreen(screen, (screen) =>
                OnNavigationToScreenComplete?.Invoke(screen));

            switch (screen)
            {
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
                case Screens.HeroesLibrary:
                    break;
                default:
                    break;
            }
        }


    }
}