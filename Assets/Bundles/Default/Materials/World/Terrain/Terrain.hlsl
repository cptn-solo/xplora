#include "HexCellData.hlsl"

// Sample appropriate terrain texture and apply cell weights and visibility.
float4 GetTerrainColor (
	UnityTexture2DArray TerrainTextures,
	float3 WorldPosition,
	float4 Terrain,
	float3 Weights,
	int index
) {
	float3 uvw = float3(WorldPosition.xz * (2 * TILING_SCALE), Terrain[index]);
	float4 c = TerrainTextures.Sample(TerrainTextures.samplerstate, uvw);
	return c * (Weights[index]);
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
	out float4 Terrain
) {
	bool editMode = false;//no need yet

	float4 cell0 = GetCellData(Indices, 0, editMode);
	float4 cell1 = GetCellData(Indices, 1, editMode);
	float4 cell2 = GetCellData(Indices, 2, editMode);

	Terrain.x = cell0.w;
	Terrain.y = cell1.w;
	Terrain.z = cell2.w;
	Terrain.w = max(max(cell0.b, cell1.b), cell2.b) * 30.0;
}

void GetFragmentData_float (
	UnityTexture2DArray TerrainTextures,
	float3 WorldPosition,
	float4 Terrain,
	float3 Weights,
	float4 HighlightColor,
	out float3 BaseColor
) {

	float4 c =
		GetTerrainColor(TerrainTextures, WorldPosition, Terrain, Weights, 0) +
		GetTerrainColor(TerrainTextures, WorldPosition, Terrain, Weights, 1) +
		GetTerrainColor(TerrainTextures, WorldPosition, Terrain, Weights, 2);

	BaseColor = c.rgb;

	HexGridData hgd = GetHexGridData(WorldPosition.xz);
	
	BaseColor = ApplyGrid(BaseColor, hgd);

	if (hgd.IsHighlighted()) {
		BaseColor = ApplyHighlight(BaseColor, HighlightColor.rgb, hgd);
	}
}
