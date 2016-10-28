Shader "Custom/Standard3Color - Flipped" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_BumpMap("Normal Map", 2D) = "bump" {}
		_PriColor ("Primary Color", Color) = (0.5, 0.5, 0.5, 1)
		_SecColor ("Secondary Color", Color) = (0.5, 0.5, 0.5, 1)
		_TerColor ("Tertiary Color", Color) = (0.5, 0.5, 0.5, 1)
		_Colormask ("Colormask", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
        Cull Front
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Colormask;
		
		
		fixed4 _PriColor;
		fixed4 _SecColor;
		fixed4 _TerColor;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			// OLD WAY
			// Albedo comes from a texture tinted by color
			// fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			// o.Albedo = c.rgb;
			
			// custom colors
			fixed4 dif = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = half4(0, 0, 0, 0);
			fixed4 cmask = tex2D(_Colormask, IN.uv_MainTex);
			o.Albedo += cmask.r*_PriColor + cmask.b*_SecColor + cmask.g*_TerColor;
			o.Albedo *= dif.rgb;
			
			o.Albedo *= _Color;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = dif.a;
			o.Normal = -1*UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
