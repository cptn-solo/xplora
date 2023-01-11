Shader "Custom/VertexColors" {
	Properties {
		//_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		//_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0

		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "white" {}
		_DetailTex ("Detail Texture", 2D) = "gray" {}
	}
	SubShader {
		//Tags { "RenderType"="Opaque" }
		//LOD 200
		Pass {
			CGPROGRAM
			//#pragma surface surf Standard fullforwardshadows
			//#pragma target 3.0

			//sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float4 color : COLOR;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			//void surf (Input IN, inout SurfaceOutputStandard o) {
			//	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			//	o.Albedo = c.rgb * IN.color;
			//	o.Metallic = _Metallic;
			//	o.Smoothness = _Glossiness;
			//	o.Alpha = c.a;
			//}

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			float4 _Tint;
			sampler2D _MainTex, _DetailTex;
			float4 _MainTex_ST, _DetailTex_ST;

			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uvDetail : TEXCOORD1;
			};

			Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.uvDetail = TRANSFORM_TEX(v.uv, _DetailTex);
				return i;
			}

			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				float4 color = tex2D(_MainTex, i.uv) * _Tint;
				color *= tex2D(_DetailTex, i.uvDetail) * unity_ColorSpaceDouble;
				return color;
			}
			ENDCG
		}

	}
	//FallBack "Diffuse"
}
