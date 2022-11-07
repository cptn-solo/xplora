using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Assets.Scripts.UI
{
    public class MenuScreen : MonoBehaviour
    {
        [SerializeField] private Screens screen;
        public Screens Screen => screen;

        private MenuNavigationService nav;
        
        private UIMenuButton[] menuButtons;
        
        [Inject]
        public void Construct(MenuNavigationService nav) =>
            this.nav = nav;


        private void Start()
        {
            menuButtons = GetComponentsInChildren<UIMenuButton>();
            foreach (var button in menuButtons)
                button.OnMenuButtonClick += Button_OnMenuButtonClick;
        }

        private void OnDestroy()
        {
            foreach (var button in menuButtons)
                button.OnMenuButtonClick -= Button_OnMenuButtonClick;
        }

        private void Button_OnMenuButtonClick(Screens screen) =>
            nav.NavigateToScreen(screen);
    }

}