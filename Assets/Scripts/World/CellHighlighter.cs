using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World.HexMap;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class CellHighlighter : MonoBehaviour
    {
        [Inject] private readonly AudioPlaybackService audioService = default;

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
        }

        public void HighlightCellAtCoordinates(
            HexCoordinates? coordinates,
            HexCoordAccessorCallback callback = null)
        {
            if (coordinates == null)
            {
                ClearCellHighlightData();
            }
            else
            {
                var coord = (HexCoordinates)coordinates;
                UpdateCoordHighlightData(coord.HexX, coord.HexZ);

                audioService.Play(CommonSoundEvent.FieldCellHover.SoundForEvent());
            }
        }


        public void ClearCellHighlightData() =>
            Shader.SetGlobalVector(cellHighlightingId, new Vector4(0f, 0f, -1f, 0f));


    }
}
