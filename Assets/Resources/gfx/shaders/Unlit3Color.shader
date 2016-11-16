Shader "Unlit/Unlit3Color"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0.5,0.5,0.5,1)
        _PriColor("Primary Color", Color) = (0.5, 0.5, 0.5, 1)
        _SecColor("Secondary Color", Color) = (0.5, 0.5, 0.5, 1)
        _TerColor("Tertiary Color", Color) = (0.5, 0.5, 0.5, 1)
        _Colormask("Colormask", 2D) = "black" {}
        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailColor("Detail Color", Color) = (0.5, 0.5, 0.5, 1)
        _Highlight("Highlight Color", Range(0.0, 1.0)) = 0.25

        [Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
            #pragma shader_feature ___ _DETAIL_MASK
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float4 _DetailMask_ST;
            sampler2D _Colormask;
            sampler2D _DetailMask;
            fixed4 _Color;
            fixed4 _PriColor;
            fixed4 _SecColor;
            fixed4 _TerColor;
            fixed4 _DetailColor;
            float _Highlight;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv2, _DetailMask);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
                fixed4 col = half4(0,0,0,0); 
				fixed4 dif = tex2D(_MainTex, i.uv);
                fixed4 cmask = tex2D(_Colormask, i.uv);
                col += cmask.r*_PriColor + cmask.b*_SecColor + cmask.g*_TerColor;
                col *= dif.rgba;
                #if _DETAIL_MASK
                    half detailMask = tex2D(_DetailMask, i.uv2).a;
                    col *= (1 - detailMask);
                    col += _DetailColor*detailMask;
                #endif
                col += half4(_Highlight, _Highlight, _Highlight, 1);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
