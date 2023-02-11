#include "HexCellData.hlsl"

// Sample appropriate terrain texture and apply
// cell weights and visibility.
float4 GetTerrainColor (
	UnityTexture2DArray TerrainTextures,
	float3 WorldPosition,
	float4 Terrain,
	float3 Weights,
	float4 Visibility,
	int index
) {
	float3 uvw = float3((WorldPosition.xz) * (2 * TILING_SCALE), Terrain[index]);
	float4 c = TerrainTextures.Sample(TerrainTextures.samplerstate, uvw);
	return c * (Weights[index] * Visibility[index]);
}

float4 GetTerrainColorUV (
	UnityTexture2DArray TerrainTextures,
	float2 cellUV,
	float4 Terrain,
	float3 Weights,
	float4 Visibility,
	int index
) {
	float3 uvw = float3(cellUV, Terrain[index]);
	float4 c = TerrainTextures.Sample(TerrainTextures.samplerstate, uvw);
	return c * (Weights[index] * Visibility[index]);
}


// Apply an 70% darkening grid outline at hex center distance 0.965-1.
float3 ApplyGrid (float3 baseColor, HexGridData h) {
	return baseColor * (1 + 0.5 * h.Smoothstep10(0.965));
}

// Apply a white outline at hex center distance 0.68-0.8.
float3 ApplyHighlight (float3 baseColor, float3 hlColor, HexGridData h) {
	return (baseColor * h.Smoothstep10(0.9)) + (hlColor * h.Smoothstep01(0.9));
}

void GetVertexCellData_float (
	float3 Indices,
	float3 Weights,
	out float4 Terrain,
	out float4 Visibility
) {
	bool editMode = false;//no need yet

	float4 cell0 = GetCellData(Indices, 0, editMode);
	float4 cell1 = GetCellData(Indices, 1, editMode);
	float4 cell2 = GetCellData(Indices, 2, editMode);

	Terrain.x = cell0.w;
	Terrain.y = cell1.w;
	Terrain.z = cell2.w;
	Terrain.w = max(max(cell0.b, cell1.b), cell2.b) * 30.0;

	Visibility.x = cell0.x;
	Visibility.y = cell1.x;
	Visibility.z = cell2.x;
	Visibility.xyz = lerp(0.5, 1, Visibility.xyz);
	Visibility.w = cell0.y * Weights.x + cell1.y * Weights.y + cell2.y * Weights.z;

}

void GetFragmentData_float (
	UnityTexture2DArray TerrainTextures,
	float3 WorldPosition,
	float4 Terrain,
	float4 Visibility,
	float3 Weights,
	float4 HighlightColor,
	out float3 BaseColor,
	out float Exploration
) {

	HexGridData hgd = GetHexGridData(WorldPosition.xz);

	float4 c =
		GetTerrainColorUV(TerrainTextures, hgd.cellUV, Terrain, Weights, Visibility, 0) +
		GetTerrainColorUV(TerrainTextures, hgd.cellUV, Terrain, Weights, Visibility, 1) +
		GetTerrainColorUV(TerrainTextures, hgd.cellUV, Terrain, Weights, Visibility, 2);

	BaseColor = c.rgb;

	bool drawGrid = false;
	if (drawGrid) {
		BaseColor = ApplyGrid(BaseColor, hgd);
	}

	if (hgd.IsHighlighted()) {
		BaseColor = ApplyHighlight(BaseColor, HighlightColor.rgb, hgd);
	}

	Exploration = Visibility.w;
}
