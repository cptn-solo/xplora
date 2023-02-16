using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public interface IHexCellGrid
    {
        HexCell GetCell(Vector3 position);
        HexCell GetCell(Ray ray);
    }
}