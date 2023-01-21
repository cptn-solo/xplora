#include "HexCellData.hlsl"

// Apply an 70% darkening grid outline at hex center distance 0.965-1.
float3 ApplyGrid (float3 baseColor, HexGridData h) {
	return baseColor * (1 + 0.5 * h.Smoothstep10(0.965));
}

// Apply a white outline at hex center distance 0.68-0.8.
float3 ApplyHighlight (float3 baseColor, float3 hlColor, HexGridData h) {
	//return saturate(h.SmoothstepRange(0.9, 0.95) + hlColor.rgb);
	//return baseColor + (hlColor * h.Smoothstep01(0.7));
	//return baseColor * h.Smoothstep10(0.85);

	//return saturate(hlColor);

	return (baseColor * h.Smoothstep10(0.9)) + (hlColor * h.Smoothstep01(0.9));
}

void GetFragmentData_float (
	float3 WorldPosition,
	float4 InColor,
	float4 HighlightColor,
	out float3 BaseColor
) {
	
	BaseColor = InColor.rgb;

	HexGridData hgd = GetHexGridData(WorldPosition.xz);

	BaseColor = ApplyGrid(BaseColor, hgd);

	if (hgd.IsHighlighted()) {
		BaseColor = ApplyHighlight(BaseColor, HighlightColor.rgb, hgd);
	}
}
