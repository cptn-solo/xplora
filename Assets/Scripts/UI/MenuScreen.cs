using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI
{
    public class MenuScreen : MonoBehaviour
    {
        [Inject] private readonly MenuNavigationService nav = default;

        [SerializeField] private Screens screen;

        private UIMenuButton[] menuButtons;

        protected Canvas canvas;
        public Screens Screen => screen;


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
            canvas = GetComponentInParent<Canvas>();

            OnBeforeStart();
            
            menuButtons = GetComponentsInChildren<UIMenuButton>(true);
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