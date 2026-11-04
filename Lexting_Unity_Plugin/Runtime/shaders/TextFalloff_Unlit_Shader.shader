Shader "HandsFree/TextFalloff_Unlit_Shader"
{
    Properties
    {
		_UpperFalloffRange("UpperFalloffRange", Range(0.0,1.0)) = 1.1
		_LowerFalloffRange("LowerFalloffRange", Range(0.0,1.0)) = 0.05
		_UpperFalloffHeight("UpperFalloffHeight", Range(-10.0,10.0)) = 0
		_LowerFalloffHeight("LowerFalloffHeight", Range(-10.0,10.0)) = 0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

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
				float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _UpperFalloffRange;
			float _LowerFalloffRange;
			float _UpperFalloffHeight;
			float _LowerFalloffHeight;

            v2f vert (appdata v)
            {
                v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				col.a = col.a * lerp(0.0, 1.0, pow(clamp((_UpperFalloffHeight - i.worldPos.y) / _UpperFalloffRange, 0.0, 1.0), 2)) * lerp(0.0, 1.0, pow(clamp((i.worldPos.y - _LowerFalloffHeight) / _LowerFalloffRange, 0.0, 1.0), 2));
                return col;
            }
            ENDCG
        }
    }
}
