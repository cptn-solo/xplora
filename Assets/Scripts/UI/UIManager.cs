using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        
        private GameObject activeScreen;
        private MenuScreen[] screens;

        [Inject]
        public void Construct(MenuNavigationService nav)
        {
            Debug.Log($"UIManager.Construct: {nav}");
            nav.UIManager = this;
        }

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            screens = GetComponentsInChildren<MenuScreen>(true);
            
            ToggleScreen(Screens.Hub);
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