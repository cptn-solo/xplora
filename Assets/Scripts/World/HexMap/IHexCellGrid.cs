using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public interface IHexCellGrid
    {
        HexCoordinates TouchCell(Vector3 position);
    }
}