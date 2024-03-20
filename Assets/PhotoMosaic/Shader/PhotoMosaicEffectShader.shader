// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/PhotoMosaicEffectShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
		_PhotoTex ("_PhotoTex", 2D) = "white" {}
		_IndexTex ("_IndexTex", 2D) = "white" {}
		_IndexTexLevel ("_IndexTexLevel", int) = 3
		_Scale ("scale", Float) = 16
		_ColorCheat ("Color Cheat", Range(0.0,1.0)) = .5
		_PhotoCheat ("Photo Cheat", Range(0.0,1.0)) = .5
		[Enum(PhotoMosaicImageEffect.Randomness)]_Randomness ("Randomness", int) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}
			float rand(float2 co){
    			return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}
			sampler2D _MainTex;
			sampler2D _PhotoTex;
			sampler2D _IndexTex;
			int _IndexTexLevel;
			float _Scale;
			float _ColorCheat;
			float _PhotoCheat;
			int _Randomness;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 ori = tex2D(_MainTex, i.uv);
				fixed2 scale = fixed2(_Scale*_ScreenParams.x/_ScreenParams.y,_Scale);
				fixed2 gridUV = floor( (i.uv)*scale);
				fixed4 col = tex2D(_MainTex, gridUV/scale);
				col *= .998;
				fixed4 index = tex2D(_IndexTex,float2(col.r/_IndexTexLevel+round(col.g*_IndexTexLevel)/_IndexTexLevel,col.b));

				//fixed selected = _Randomness==0?index[0]:index[int(((gridUV.x+gridUV.y))%_Randomness)];
				fixed selected = _Randomness==0?index[0]:index[int(rand(gridUV)*_Randomness)];
				selected = round(selected*255);

				//fixed4 col2 = tex2D(_PhotoTex, ((i.uv-.5)*scale+ceil(scale))%1/16+float2(selected%16,floor(selected/16) )/16); 
				fixed4 col2 = tex2D(_PhotoTex,(i.uv*scale-gridUV)/16+float2(selected%16,floor(selected/16))/16);
				//return fixed4(selected%16/16,floor(selected/16)/16,0,0);
				return lerp(lerp(col2,col,_ColorCheat),ori,_PhotoCheat);
			}
			ENDCG
		}
	}
}
