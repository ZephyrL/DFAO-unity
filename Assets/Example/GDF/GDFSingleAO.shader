Shader "GDF/GDFSingleAO"
{
    Properties
    {
		_baseColor("Base Color" , Color) = (0.2, 0.5, 0.8, 1)

		_aoStepSize("AO Step Size", Range(0.05, 0.5)) = 0.15	
		_aoMaxIter("AO Iterations", Range(1, 5)) = 3
		_aoIntensity("AO Intensity", Range(-0.3, 0.5)) = 0.3

		_shadowPenumbra("Shadow Penumbra", Range(4, 40)) = 8

		_accuracy("Ray March Accuracy", Range(0.001, 0.01)) = 0.003
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

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 pos : TEXCOORD0;
				float3 normal : TEXCOORD1;
            };

			Texture3D _GlobalDFTex;
			float3 _AABBmin, _AABBmax;
			StructuredBuffer<VolumeData> _VolumeData;
			int bufferLength;
			Texture3D _TextureAtlas;	// local sdf's
			int textureCount;

			float sdfRough(float3 pos){
				return GDFTex3D(pos, _AABBmin, _AABBmax, _GlobalDFTex);
			}

			float sdf(float3 p){
				float dist = 1000;
				float temp;
				for(int i = 0; i < bufferLength; i++){
					if(GDFInsideAABB(p, _VolumeData[i])){
						temp = GDFAtlasTex3D(p, _VolumeData[i], textureCount, _TextureAtlas);
						dist = min(dist, temp);
					}
				}
				if(dist < 1000) return dist;
				temp = GDFTex3D(p, _AABBmin, _AABBmax, _GlobalDFTex);
				dist = min(temp, dist);
				return dist;
			}

			float3 getNormal(float3 p) {
				const float2 offset = float2(0.001, 0.0);
				float3 normal = float3(
					sdf(p + offset.xyy) - sdf(p - offset.xyy),
					sdf(p + offset.yxy) - sdf(p - offset.yxy),
					sdf(p + offset.yyx) - sdf(p - offset.yyx)
					);
				return normalize(normal);
			}

			float _aoStepSize;
			int _aoMaxIter;
			float _aoIntensity;

			float AmbientOcclusion(float3 p, float3 n){
				float step = _aoStepSize;
				float ao = 0.0;
				float dist;
				for(int i = 1; i <= _aoMaxIter; i++){
					dist = step * i;
					ao += max((dist - sdfRough(p + n * dist)) / dist, 0.0);
				}
				return (1.0 - ao * _aoIntensity);
			}

			float _accuracy;
			float _shadowPenumbra;

			float softShadow(float3 pos){
				// currently directional light
				float result = 1.0f;
				float maxDistance = 10.0;
				float step = maxDistance / 10;
				float3 dir = normalize(_WorldSpaceLightPos0.xyz);
				float t = 0.1;
				[loop]
				while(t < maxDistance){
					//float2 d = sdfConfident(pos + dir * t);
					float d = sdfRough(pos + dir * t);
					if(d.r < _accuracy){
						return 0.0;
					}
					result = min(result, _shadowPenumbra * d.r / t);
					//t += min(d, step);
					t += d.r;
				}
				return result;
			}

			float4 _baseColor;

			fixed4 Shading(float3 pos, float3 normal){

				float4 light = dot(_WorldSpaceLightPos0, float4(normal, 0.0)) * 1.0;

				float ao = AmbientOcclusion(pos, normal);

				float shadow = softShadow(pos);

				float4 result = _baseColor * shadow /* * light */ * ao;

				return fixed4(result.xyz, 1.0);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
				//o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return Shading(i.pos, i.normal);
				//return fixed4(i.normal, 1);
            }
            ENDCG
        }
    }
}
