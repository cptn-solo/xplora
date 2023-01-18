using Assets.Scripts.Services;
using Assets.Scripts.World.HexMap;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Assets.Scripts.World
{
    /// <summary>
    /// Keyboard/gamepad input capture and pass to the wold service for processing
    /// </summary>
    public class PointerHandler : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;

        private PlayerInputActions input;
        
        private IHexCellGrid grid;
        private CellHighlighter cellHighlighter;
        private Vector2 pos;
        private HexCell previousCell;

        private void Awake()
        {
            input = new();
            grid = GetComponent<IHexCellGrid>();
            cellHighlighter = GetComponent<CellHighlighter>();
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
            pos = obj.ReadValue<Vector2>();
        }

        private HexCell GetCellUnderCursor() =>
            grid.GetCell(Camera.main.ScreenPointToRay(pos));
        

        private void Update()
        {
            var overUI = EventSystem.current.IsPointerOverGameObject();
            if (overUI)
                return;

            var lmb = Mouse.current.leftButton.wasPressedThisFrame;
            if (lmb)
            {
                HandleTouch();
            }
            else
            {
                HandleHover();
            }
        }

        #region Touch Cell       

        private void HandleHover()
        {
            HexCell currentCell = GetCellUnderCursor();
            if (currentCell)
            {
                previousCell = currentCell;
            }
            else
            {
                previousCell = null;
            }
            cellHighlighter.UpdateCellHighlightData(currentCell);
        }

        private void HandleTouch()
        {
            HexCell currentCell = GetCellUnderCursor();
            if (currentCell)
            {
                worldService.ProcessTargetCoordinatesSelection(currentCell.coordinates);
            }
        }

        #endregion

    }
}