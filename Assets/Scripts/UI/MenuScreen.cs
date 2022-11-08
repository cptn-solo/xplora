using UnityEngine;
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

        protected virtual void OnBeforeAwake() { }
        protected virtual void OnBeforeStart() { }
        protected virtual void OnBeforeUpdate() { }
        protected virtual void OnBeforeDestroy() { }
        protected virtual void OnBeforeEnable() { }
        protected virtual void OnBeforeDisable() { }

        private void Awake() =>
            OnBeforeAwake();

        private void Start()
        {
            OnBeforeStart();
            
            menuButtons = GetComponentsInChildren<UIMenuButton>();
            foreach (var button in menuButtons)
                button.OnMenuButtonClick += Button_OnMenuButtonClick;
            
        }

        private void Update() =>
            OnBeforeUpdate();

        private void OnDestroy()
        {
            OnBeforeDestroy();
            
            if (menuButtons != null)
                foreach (var button in menuButtons)
                    button.OnMenuButtonClick -= Button_OnMenuButtonClick;
        }

        private void OnEnable() =>
            OnBeforeEnable();

        private void OnDisable() =>
            OnBeforeDisable();

        private void Button_OnMenuButtonClick(Screens screen) =>
            nav.NavigateToScreen(screen);
    }

}