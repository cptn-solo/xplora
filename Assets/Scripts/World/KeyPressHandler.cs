using Assets.Scripts.Services;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class KeyPressHandler : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;

        private PlayerInputActions input;
        private Vector3 direction = Vector3.zero;
        private bool isListening;

        private readonly WaitForSeconds keyboardWait = new(.1f);

        private void Awake()
        {
            input = new();
        }

        private void OnEnable()
        {
            input.World.Move.performed += Move_performed;
            input.World.Move.canceled += Move_canceled;

            input.World.Gamepad.performed += Gamepad_performed;
            input.World.Gamepad.canceled += Gamepad_canceled;

            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();

            input.World.Move.performed -= Move_performed;
            input.World.Move.canceled -= Move_canceled;

            input.World.Gamepad.performed -= Gamepad_performed;
            input.World.Gamepad.canceled -= Gamepad_canceled;

            isListening = false;
            StopCoroutine(DelayedDirectionSelectionCoroutine());
        }

        private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var move = obj.ReadValue<Vector2>();
            direction = Vector3.forward * move.y + Vector3.right * move.x;
            // 0. flag move processing
            // 1. decide on target based on current position
            // 2. move focus to target
            // 3. unflag move processing

            //worldService.ProcessDirectionSelection(direction);
            if (!isListening)
                StartCoroutine(DelayedDirectionSelectionCoroutine());
            
            Debug.Log($"Move_performed {move}");
        }

        private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            direction = Vector3.zero;

            Debug.Log($"Move_canceled");
        }

        private void Gamepad_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log($"Gamepad_canceled");
        }

        private void Gamepad_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var move = obj.ReadValue<Vector2>();
            //            direction = Vector3.forward * move.y + Vector3.right * move.x;

            Debug.Log($"Gamepad_performed {move}");
        }

        private IEnumerator DelayedDirectionSelectionCoroutine()
        {
            isListening = true;

            while (isListening && direction != Vector3.zero)
            {
                worldService.ProcessDirectionSelection(direction);

                yield return keyboardWait;

                direction = Vector3.zero;
            }

            isListening = false;
        }

    }
}