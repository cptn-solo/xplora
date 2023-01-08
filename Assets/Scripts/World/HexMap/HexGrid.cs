using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public class HexGrid : MonoBehaviour
    {
        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;
        [SerializeField] private HexCell cellPrefab;
        [SerializeField] private TextMeshProUGUI cellLabelPrefab;

        private Canvas gridCanvas;
        private HexMesh hexMesh;

        private HexCell[] cells;

        private void Awake()
        {
            gridCanvas = GetComponentInChildren<Canvas>();
            hexMesh = GetComponentInChildren<HexMesh>();

            cells = new HexCell[height * width];

            for (int z = 0, i = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }
        void Start()
        {
            hexMesh.Triangulate(cells);
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

            TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab);
            label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = x.ToString() + "\n" + z.ToString();
        }
    }
}