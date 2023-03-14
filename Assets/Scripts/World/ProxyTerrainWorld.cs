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
        private CellHighlighter cellHighlighter;

        private void Awake()
        {
            grid = GetComponent<HexGrid>();
            cellHighlighter = GetComponent<CellHighlighter>();
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

    }
}