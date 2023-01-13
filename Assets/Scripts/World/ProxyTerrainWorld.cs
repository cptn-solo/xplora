using Assets.Scripts.Services;
using Assets.Scripts.World.HexMap;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class ProxyTerrainWorld : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;
        
        private HexGrid grid;

        private void Awake()
        {
            grid = GetComponent<HexGrid>();
        }
        private void Start()
        {
            worldService.CoordResolver = ResolveHexCoordinates;
            worldService.WorldPositionResolver = ResolveWorldPosition;
            
            worldService.CoordHighlighter = grid.HighlightTargetedCell;            
            worldService.TerrainProducer = grid.ProduceCells;
            worldService.CellIndexResolver = grid.CellIndexForCoordinates;
            worldService.CellCoordinatesResolver = grid.CellCoordinatesForIndex;
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