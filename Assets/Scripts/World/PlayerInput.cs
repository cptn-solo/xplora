﻿using Assets.Scripts.Services;
using Assets.Scripts.World.HexMap;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Assets.Scripts.World
{
    public class PlayerInput : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;

        private PlayerInputActions input;
        private IHexCellGrid grid;

        private void Awake()
        {
            input = new();
             grid = GetComponent<IHexCellGrid>();
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

        private void Pointer_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log($"Pointer_canceled");
        }

        private void Pointer_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var pos = obj.ReadValue<Vector2>();
        }
        #region Touch Cell

        private void Update()
        {
            var lmb = Mouse.current.leftButton.wasPressedThisFrame;
            if (lmb)
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();

            Ray inputRay = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                HexCoordinates coordinates = grid.TouchCell(hit.point);
                worldService.ProcessMoveToHexCoordinates(coordinates);

            }
        }


        #endregion

    }
}