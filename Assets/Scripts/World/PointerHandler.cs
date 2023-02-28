using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.World.HexMap;
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
        private Vector2 pos;
        private HexCell previousCell;

        private bool pointerMoved;

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
        }

        private void Pointer_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            pointerMoved = true;
            pos = obj.ReadValue<Vector2>();
        }

        private HexCell GetCellUnderCursor() =>
            grid.GetCell(Camera.main.ScreenPointToRay(pos));
        

        private void Update()
        {
            var needHoverProcess = pointerMoved;

            pointerMoved = false;

            if (worldService.PlayerUnit == null)
                return;

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
                if (needHoverProcess)
                    HandleHover();
            }
        }

        #region Touch Cell       

        private void HandleHover()
        {
            HexCell currentCell = GetCellUnderCursor();

            bool needUpdateHighlight = false;

            if (currentCell)
            {
                if (!previousCell || currentCell != previousCell)
                    needUpdateHighlight = true;

                previousCell = currentCell;

                needUpdateHighlight = needUpdateHighlight && worldService.CheckIfReachable(currentCell.coordinates);
            }
            else
            {
                if (previousCell)
                    needUpdateHighlight = true;

                previousCell = null;
            }

            if (needUpdateHighlight)
            {
                worldService.SetAimToCoordinates(currentCell ?
                    currentCell.coordinates : null);
            }
        }

        private void HandleTouch()
        {
            HexCell currentCell = GetCellUnderCursor();
            if (currentCell)
            {
                worldService.ResetHexDir(); //to prevent next cell selection - pointer doesn't need it
                worldService.ProcessTargetCoordinatesSelection(currentCell.coordinates);                
            }
        }

        #endregion

    }
}