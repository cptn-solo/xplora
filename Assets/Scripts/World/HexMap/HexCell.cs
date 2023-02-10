using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.World.HexMap
{
    public class HexCell : MonoBehaviour, IVisibility
    {
        public HexCoordinates coordinates;
        public Color color;
        private int terrainTypeIndex;
        private int visibility;

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

        public bool IsVisible =>
            visibility > 0;

        public bool IsExplored { get; internal set; }

        [SerializeField]
        HexCell[] neighbors;

        public HexCell[] Neighbors => neighbors;

        public HexCell GetNeighbor(HexDirection direction)
        {
            return neighbors[(int)direction];
        }
        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }

        /// <summary>
        /// Reset visibility level to zero.
        /// </summary>
        public void ResetVisibility()
        {
            if (visibility > 0)
            {
                visibility = 0;
                ShaderData.RefreshVisibility(this);
            }
        }

        /// <summary>
        /// Increment visibility level.
        /// </summary>
        public void IncreaseVisibility()
        {
            visibility += 1;
            if (visibility == 1)
            {
                IsExplored = true;
                ShaderData.RefreshVisibility(this);
            }
        }

        /// <summary>
        /// Decrement visiblility level.
        /// </summary>
        public void DecreaseVisibility()
        {
            visibility -= 1;
            if (visibility == 0)
            {
                ShaderData.RefreshVisibility(this);
            }
        }

    }
}