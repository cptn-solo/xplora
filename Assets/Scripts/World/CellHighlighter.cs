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
                return;
            }

            // Works up to brush size 6.
            var cellVector = new Vector4(
                    cell.coordinates.HexX,
                    cell.coordinates.HexZ,
                    0.5f,
                    0
                );

            Shader.SetGlobalVector(
                cellHighlightingId,
                cellVector
            );
            Debug.Log($"UpdateCellHighlightData {cellVector}");
        }

        public void ClearCellHighlightData() =>
            Shader.SetGlobalVector(cellHighlightingId, new Vector4(0f, 0f, -1f, 0f));


    }
}
