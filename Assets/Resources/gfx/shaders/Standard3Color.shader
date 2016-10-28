Shader "Custom/Standard3Color"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_PriColor ("Primary Color", Color) = (0.5, 0.5, 0.5, 1)
		_SecColor ("Secondary Color", Color) = (0.5, 0.5, 0.5, 1)
		_TerColor ("Tertiary Color", Color) = (0.5, 0.5, 0.5, 1)
		_Colormask ("Colormask", 2D) = "black" {}
		
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		_ParallaxMap ("Height Map", 2D) = "black" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		
		_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}
		
		_DetailColor("Detail Color", Color) = (0.5, 0.5, 0.5, 1)

		[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


		// Blending state
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup
	ENDCG

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		#pragma shader_feature _METALLICGLOSSMAP
		#pragma shader_feature _NORMALMAP
		#pragma shader_feature ___ _DETAIL_MULX2
		
		#pragma surface surf Standard fullforwardshadows
		
		// Standard Input
		//---------------------------------------
		half4		_Color;
		half		_Cutoff;

		sampler2D	_MainTex;

		sampler2D	_DetailAlbedoMap;

		sampler2D	_BumpMap;
		half		_BumpScale;

		sampler2D	_DetailMask;
		sampler2D	_DetailNormalMap;
		half		_DetailNormalMapScale;

		sampler2D	_SpecGlossMap;
		sampler2D	_MetallicGlossMap;
		half		_Metallic;
		half		_Glossiness;

		sampler2D	_OcclusionMap;
		half		_OcclusionStrength;

		sampler2D	_ParallaxMap;
		half		_Parallax;
		half		_UVSec;

		half4 		_EmissionColor;
		sampler2D	_EmissionMap;
		//--------------------------------------


		sampler2D _Colormask;
		fixed4 _PriColor;
		fixed4 _SecColor;
		fixed4 _TerColor;
		fixed4 _DetailColor;
		

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv2_DetailMask;
			float2 uv2_DetailAlbedoMap;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			fixed4 dif = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = half4(0, 0, 0, 0);
			fixed4 cmask = tex2D(_Colormask, IN.uv_MainTex);
			o.Albedo += cmask.r*_PriColor + cmask.b*_SecColor + cmask.g*_TerColor;
			o.Albedo += dif.rgb;
			
			o.Albedo *= _Color;
			
			#if _DETAIL_MULX2
				fixed4 detailAlbedo = tex2D(_DetailAlbedoMap, IN.uv2_DetailAlbedoMap);
				half detailMask = tex2D(_DetailMask, IN.uv2_DetailMask).a;
				o.Albedo *= (1 - detailMask);
				
				o.Albedo += _DetailColor*detailAlbedo.rgb*detailMask;
				
			#endif
			
			#if _METALLICGLOSSMAP
				fixed4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
				
				o.Metallic = mg.r;
				o.Smoothness = mg.a;
			#else
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
			#endif
			
			
			o.Alpha = dif.a;
			
			#if _NORMALMAP
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			#endif
		}
		ENDCG
	
	}

	FallBack "VertexLit"
	CustomEditor "ThreeColorStandardShaderGUI"
}
