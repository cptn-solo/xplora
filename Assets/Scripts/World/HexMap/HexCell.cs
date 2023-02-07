using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
        public Color color;
        int terrainTypeIndex;

        /// <summary>
        /// Unique global index of the cell.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Terrain type index.
        /// </summary>
        public int TerrainTypeIndex
        {
            get => terrainTypeIndex;
            set
            {
                if (terrainTypeIndex != value)
                {
                    terrainTypeIndex = value;
                    ShaderData.RefreshTerrain(this);
                }
            }
        }

        /// <summary>
        /// Reference to <see cref="HexCellShaderData"/> that contains the cell.
        /// </summary>
        public HexCellShaderData ShaderData { get; set; }
        public bool IsUnderwater { get; internal set; } = false;
        public float WaterSurfaceY { get; internal set; } = -100f;
        public bool IsVisible { get; internal set; } = true;
        public bool IsExplored { get; internal set; } = true;

        [SerializeField]
        HexCell[] neighbors;

        public HexCell GetNeighbor(HexDirection direction)
        {
            return neighbors[(int)direction];
        }
        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }
    }
}