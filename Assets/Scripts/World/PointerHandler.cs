using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace Assets.Scripts.World
{
    /// <summary>
    /// Keyboard/gamepad input capture and pass to the wold service for processing
    /// </summary>
    public class PointerHandler : MonoBehaviour
    {
        private PlayerInputActions input;
        
        private Vector2 pos;

        private bool pointerMoved;

        public event UnityAction<Vector2> OnPositionHover;
        public event UnityAction<Vector2> OnPositionTouch;

        private void Awake()
        {
            input = new();
        }

        private void OnEnable()
        {
            input.Enable();

            input.World.Pointer.performed += Pointer_performed;
            input.World.Pointer.canceled += Pointer_canceled;
        }

        private void OnDisable()
        {
            input.Disable();

            input.World.Pointer.performed -= Pointer_performed;
            input.World.Pointer.canceled -= Pointer_canceled;

        }

        private void Pointer_canceled(InputAction.CallbackContext obj)
        {
        }

        private void Pointer_performed(InputAction.CallbackContext obj)
        {
            pointerMoved = true;
            pos = obj.ReadValue<Vector2>();
        }

        

        private void Update()
        {
            var needHoverProcess = pointerMoved;

            pointerMoved = false;

            var overUI = EventSystem.current.IsPointerOverGameObject();
            if (overUI)
                return;

            var lmb = Mouse.current.leftButton.wasPressedThisFrame;
            if (lmb)
            {
                OnPositionTouch?.Invoke(pos);
            }
            else
            {
                if (needHoverProcess)
                    OnPositionHover(pos);
            }
        }



    }
}