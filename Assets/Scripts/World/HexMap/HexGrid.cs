using Assets.Scripts.Services;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public class HexGrid : MonoBehaviour, IHexCellGrid
    {
        private int width = 6;
        private int height = 6;

        /// <summary>
        /// Amount of cells in the X dimension.
        /// </summary>
        public int CellCountX => width;

        /// <summary>
        /// Amount of cells in the Z dimension.
        /// </summary>
        public int CellCountZ => height;

        [SerializeField] private HexCell cellPrefab;
        [SerializeField] private TextMeshProUGUI cellLabelPrefab;

        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color touchedColor = Color.magenta;
        
        private Canvas gridCanvas;
        private HexMesh hexMesh;

        private HexCell[] cells;

        private void Awake()
        {
            gridCanvas = GetComponentInChildren<Canvas>();
            hexMesh = GetComponentInChildren<HexMesh>();            
        }

        /// <summary>
        /// Please call not earlier then from Start()
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public void ProduceCells(int height, int width, TerrainProducerCallback callback)
        {
            this.height = height;
            this.width = width;

            cells = new HexCell[height * width];

            for (int z = 0, i = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
            hexMesh.Triangulate(cells);

            callback?.Invoke();
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            //position.x = x * (HexMetrics.innerRadius * 2f); // square
            position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f); // hex
            position.y = 0f;
            position.z = z * (HexMetrics.outerRadius * 1.5f);

            HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.color = defaultColor;

            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                    if (x < width - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                    }
                }
            }

            TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab);
            label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
        }

        public HexCoordinates TouchCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);

            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            Debug.Log("touched at " + coordinates.ToString());

            return coordinates;
        }

        public void HighlightTargetedCell(HexCoordinates coordinates)
        {
            HexCell cell = CellForCoordinates(coordinates);
            cell.color = touchedColor;
            hexMesh.Triangulate(cells);
        }
        public HexCoordinates CellCoordinatesForIndex(int index)
        {
            HexCell cell = cells[index];
            return cell.coordinates;
        }
        public int CellIndexForCoordinates(HexCoordinates coordinates)
        {
            int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
            return index;
        }
        public HexCell CellForCoordinates(HexCoordinates coordinates)
        {
            int index = CellIndexForCoordinates(coordinates);
            HexCell cell = cells[index];
            return cell;
        }

        /// <summary>
        /// Get a cell given a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray"><see cref="Ray"/> used to perform a raycast.</param>
        /// <returns>The hit cell, if any.</returns>
        public HexCell GetCell(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                return GetCell(hit.point);
            }
            return null;
        }

        /// <summary>
        /// Get the cell that contains a position.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <returns>The cell containing the position, if it exists.</returns>
        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            return GetCell(coordinates);
        }

        /// <summary>
        /// Get the cell with specific <see cref="HexCoordinates"/>.
        /// </summary>
        /// <param name="coordinates"><see cref="HexCoordinates"/> of the cell.</param>
        /// <returns>The cell with the given coordinates, if it exists.</returns>
        public HexCell GetCell(HexCoordinates coordinates)
        {
            int z = coordinates.Z;
            if (z < 0 || z >= CellCountZ)
            {
                return null;
            }
            int x = coordinates.X + z / 2;
            if (x < 0 || x >= CellCountX)
            {
                return null;
            }
            return cells[x + z * CellCountX];
        }
    }
}