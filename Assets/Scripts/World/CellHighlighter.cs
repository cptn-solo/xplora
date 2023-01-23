using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class CellHighlighter : MonoBehaviour
    {
        static readonly int cellHighlightingId =
            Shader.PropertyToID("_CellHighlighting");

        public void UpdateCellHighlightData(HexCell cell)
        {
            if (cell == null)
            {
                ClearCellHighlightData();
                Debug.Log($"ClearCellHighlightData");
                return;
            }

            UpdateCoordHighlightData(cell.coordinates.HexX, cell.coordinates.HexZ);

        }

        private void UpdateCoordHighlightData(float hexX, float hexZ)
        { 
            // Works up to brush size 6.
            var cellVector = new Vector4(
                    hexX,
                    hexZ,
                    0.5f,
                    0
                );

            Shader.SetGlobalVector(
                cellHighlightingId,
                cellVector
            );
            Debug.Log($"UpdateCellHighlightData {cellVector}");
        }

        public void HighlightCellAtCoordinates(HexCoordinates? coordinates)
        {
            if (coordinates == null)
            {
                ClearCellHighlightData();
            }
            else
            {
                var coord = (HexCoordinates)coordinates;
                UpdateCoordHighlightData(coord.HexX, coord.HexZ);
            }
        }


        public void ClearCellHighlightData() =>
            Shader.SetGlobalVector(cellHighlightingId, new Vector4(0f, 0f, -1f, 0f));


    }
}
