using Assets.Scripts.Services;
using Assets.Scripts.World.HexMap;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class ProxyTerrainWorld : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService = default;
        
        private HexGrid grid;
        private HexCell previousCell;
        private CellHighlighter cellHighlighter;
        private PointerHandler pointerHandler;

        private void Awake()
        {
            pointerHandler = GetComponent<PointerHandler>();
            grid = GetComponent<HexGrid>();
            cellHighlighter = GetComponent<CellHighlighter>();

            pointerHandler.OnPositionHover += HandleHover;
            pointerHandler.OnPositionTouch += HandleTouch;
        }

        private void Start()
        {

            worldService.CoordResolver = ResolveHexCoordinates;
            worldService.WorldPositionResolver = ResolveWorldPosition;

            worldService.CoordHoverer = cellHighlighter.HighlightCellAtCoordinates;

            worldService.TerrainProducer = grid.ProduceCells;
            worldService.CellIndexResolver = grid.CellIndexForCoordinates;
            worldService.CellCoordinatesResolver = grid.CellCoordinatesForIndex;
        }

        private void OnDestroy()
        {
            worldService.CoordResolver = null;
            worldService.WorldPositionResolver = null;

            worldService.CoordHoverer = null;

            worldService.TerrainProducer = null;
            worldService.CellIndexResolver = null;
            worldService.CellCoordinatesResolver = null;

            pointerHandler.OnPositionHover -= HandleHover;
            pointerHandler.OnPositionTouch -= HandleTouch;
        }

        private HexCoordinates ResolveHexCoordinates(HexCoordinates coord, HexDirection dir)
        {
            var cell = grid.CellForCoordinates(coord);
            
            if (cell == null || dir == HexDirection.NA)
                return coord;

            var neighbor = cell.GetNeighbor(dir);
            
            if (neighbor == null)
                return cell.coordinates;

            return neighbor.coordinates;
        }

        private Vector3 ResolveWorldPosition(HexCoordinates coord, HexDirection dir)
        {
            if (dir == HexDirection.NA)
                return coord.ToPosition();

            var cell = grid.CellForCoordinates(coord);

            if (cell == null)
                return coord.ToPosition();

            var neighbor = cell.GetNeighbor(dir);

            if (neighbor == null)
                return cell.coordinates.ToPosition();

            return neighbor.coordinates.ToPosition();
        }

        private HexCell GetCellUnderCursor(Vector2 pos) =>
            grid.GetCell(Camera.main.ScreenPointToRay(pos));

        #region Touch Cell       

        private void HandleHover(Vector2 pos)
        {

            if (worldService.PlayerUnit == null)
                return;

            HexCell currentCell = GetCellUnderCursor(pos);

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

        private void HandleTouch(Vector2 pos)
        {
            if (worldService.PlayerUnit == null)
                return;

            HexCell currentCell = GetCellUnderCursor(pos);
            if (currentCell)
            {
                worldService.ResetHexDir(); //to prevent next cell selection - pointer doesn't need it
                worldService.ProcessTargetCoordinatesSelection(currentCell.coordinates);
            }
        }

        #endregion
    }
}