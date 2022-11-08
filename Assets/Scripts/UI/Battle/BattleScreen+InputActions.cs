using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Input Actions
    {
        private DefaultInputActions actions;
        private bool tracking = false;

        private void InitInputActions()
        {
            actions = new DefaultInputActions();
        }

        private void EnableInputActions()
        {
            //actions.UI.Click.performed += Click_performed;
            //actions.Enable();
        }

        private void DisableInputActions()
        {
            //actions.UI.Click.performed -= Click_performed;
            //actions.Disable();
        }

        private void ProcessInputActions()
        {
            if (tracking)
                Debug.Log(actions.UI.Point.ReadValue<Vector2>());
        }
        private void Click_performed(InputAction.CallbackContext obj)
        {
            Debug.Log($"Click_performed: {obj}");
            tracking = obj.ReadValue<float>() > 0;
        }

    }

}