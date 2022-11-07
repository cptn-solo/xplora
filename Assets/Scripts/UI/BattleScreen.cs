using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI
{
    public class BattleScreen : MenuScreen
    {
        private DefaultInputActions actions;

        private bool tracking = false;

        private void Awake()
        {
            actions = new DefaultInputActions();
        }
        private void OnEnable()
        {
            actions.UI.Click.performed += Click_performed;
            actions.UI.Click.started += Click_started;
            actions.UI.Click.canceled += Click_canceled;
            actions.UI.Point.started += Point_started;
            actions.UI.Point.canceled += Point_canceled;
            actions.Enable();
        }

        private void Point_canceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"Point_canceled: {obj}");
        }

        private void Point_started(InputAction.CallbackContext obj)
        {
            Debug.Log($"Point_started: {obj}");
        }

        private void OnDisable()
        {
            actions.UI.Click.performed -= Click_performed;
            actions.UI.Click.started -= Click_started;
            actions.UI.Click.canceled -= Click_canceled;
            actions.UI.Point.started -= Point_started;
            actions.UI.Point.canceled -= Point_canceled;
            actions.Disable();
        }

        private void Click_canceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"Click_canceled: {obj}");
        }

        private void Click_started(InputAction.CallbackContext obj)
        {
            Debug.Log($"Click_started: {obj}");
        }

        private void Click_performed(InputAction.CallbackContext obj)
        {
            Debug.Log($"Click_performed: {obj}");
            tracking = obj.ReadValue<float>() > 0;
        }

        private void Update()
        {
            if (tracking)
                Debug.Log(actions.UI.Point.ReadValue<Vector2>());
        }

    }


}