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

        [Inject]
        public void Construct(MenuNavigationService nav) =>
            nav.UIManager = this;

        private void Awake() =>
            DontDestroyOnLoad(this.gameObject);

        private void Start()
        {
            screens = GetComponentsInChildren<MenuScreen>(true);

            foreach (var screen in screens)
                screen.gameObject.SetActive(false);

            //ToggleScreen(Screens.Hub);
            //ToggleScreen(Screens.Battle);
            ToggleScreen(Screens.HeroesLibrary);

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