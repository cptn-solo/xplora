using Assets.Scripts.Services;
using Assets.Scripts.World.HexMap;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class GroundWorldProxy : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;
        private HexGrid grid;

        private void Awake()
        {
            grid = GetComponent<HexGrid>();
        }
        private void Start()
        {
            worldService.SetCoordResolver(Resolve);
        }

        private HexCoordinates Resolve(HexCoordinates coord, HexDirection dir)
        {
            var cell = grid.CellForCoordinates(coord);
            
            if (cell == null)
                return new HexCoordinates(0, 0);

            var neighbor = cell.GetNeighbor(dir);
            
            if (neighbor == null)
                return cell.coordinates;

            return neighbor.coordinates;
        }

    }
}