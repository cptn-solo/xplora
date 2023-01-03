using UnityEngine;

namespace Assets.Scripts.World
{
    public class UnitMovement : MonoBehaviour
    {
        private PlayerInputActions input;

        private void Awake()
        {
            input = new();
        }

        private void OnEnable()
        {
            input.World.Move.performed += Move_performed;
            input.World.Move.started += Move_started;
            input.World.Move.canceled += Move_canceled;

            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();

            input.World.Move.performed -= Move_performed;
            input.World.Move.started -= Move_started;
            input.World.Move.canceled -= Move_canceled;
        }

        private void Move_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log($"Move_started  {obj.ReadValue<Vector2>()}");

        }

        private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log("Move_canceled");
        }

        private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log($"Move_performed {obj.ReadValue<Vector2>()}");
        }



    }
}