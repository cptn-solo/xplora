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
