using Assets.Scripts.Services;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class UnitMovement : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;

        private PlayerInputActions input;
        private Vector3 direction = Vector3.zero;
        private bool isListening;

        private readonly WaitForSeconds keyboardWait = new(.2f);

        private void Awake()
        {
            input = new();
        }

        private void OnEnable()
        {
            input.World.Move.performed += Move_performed;
            input.World.Move.canceled += Move_canceled;

            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();

            input.World.Move.performed -= Move_performed;
            input.World.Move.canceled -= Move_canceled;

            isListening = false;
            StopCoroutine(DelayedMoveCoroutine());
        }

        private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var move = obj.ReadValue<Vector2>();
            direction = Vector3.forward * move.y + Vector3.right * move.x;

            if (!isListening)
                StartCoroutine(DelayedMoveCoroutine());
            
            Debug.Log($"Move_performed {move}");
        }

        private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            direction = Vector3.zero;

            Debug.Log($"Move_canceled");
        }

        private IEnumerator DelayedMoveCoroutine()
        {
            isListening = true;

            while (isListening && direction != Vector3.zero)
            {
                yield return keyboardWait;

                worldService.ProcessMoveInput(direction);
            }

            isListening = false;
        }

    }
}