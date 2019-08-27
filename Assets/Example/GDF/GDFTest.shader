Shader "GDF/GDFTest"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Assets/Example/GDF/GDFTexturing.hlsl"

			float3 _AABBmin, _AABBmax;
			Texture3D _GlobalDFTex;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 globalPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
				o.globalPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				if(!GDFInsideAABB(i.globalPos, _AABBmin, _AABBmax)) {
					return float4(0.2, 0.5, 0.8, 1);
				}
                float col = GDFTex3D(i.globalPos, _AABBmin, _AABBmax, _GlobalDFTex);

				return float4(normalize(float3(col, 0 , 1)), 1.0);
            }
            ENDCG
        }
    }
}
