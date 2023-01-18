// Cell highlighting data, in hex space.
// x: Highlight center X position.
// y: Highlight center Z position.
// z: Highlight radius, squared with bias. Is negative if there is no highlight.
// w: Hex grid wrap size, to support X wrapping. Is zero if there is no wrapping.
float4 _CellHighlighting;

// Hex grid data derived from world-space XZ position.
struct HexGridData {
	// Cell center in hex space.
	float2 cellCenter;

	// Hex grid data derived from world-space XZ position.
	bool IsHighlighted () {
		float2 cellToHighlight = abs(_CellHighlighting.xy - cellCenter);

		// Adjust for world X wrapping if needed.
		if (cellToHighlight.x > _CellHighlighting.w * 0.5) {
			cellToHighlight.x -= _CellHighlighting.w;
		}

		return dot(cellToHighlight, cellToHighlight) < _CellHighlighting.z;
	}
};

void GetFragmentData_float (
	float3 WorldPosition,
	UnityTexture2D GridTexture,
	out float3 BaseColor
) {

	float4 grid = 1;
	float2 gridUV = WorldPosition.xz;
	gridUV.x *= 1 / (4 * 8.66025404);
	gridUV.y *= 1 / (2 * 15.0);
	grid = GridTexture.Sample(GridTexture.samplerstate, gridUV);

	BaseColor = grid.rgb;
}
