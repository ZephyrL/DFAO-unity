Shader "GDF/GDFCameraScreenAO"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
	    Cull Off ZWrite Off ZTest Always

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			//#pragma target 3.0

            #include "UnityCG.cginc"
			#include "Assets/Example/GDF/GDFTexturing.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 ray : TEXCOORD1;
            };

			uniform sampler2D _CameraDepthTexture; // built-in
			uniform float4x4 _CamFrustum, _CamToWorld;

            v2f vert (appdata v)
            {
                v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				
				o.ray = _CamFrustum[(int)index].xyz;
				o.ray /= abs(o.ray.z);
				o.ray = mul(_CamToWorld, o.ray);

                return o;
            }

			float3 _AABBmin, _AABBmax;
			StructuredBuffer<VolumeData> _VolumeData;
			int bufferLength;
			Texture3D _TextureAtlas;	// local SDF's
			int textureCount;
			Texture3D _GlobalDFTex;		// gloabl SDF
			
			float sdfRough(float3 p){
				return GDFTex3D(p, _AABBmin, _AABBmax, _GlobalDFTex);
			}

			float sdfRoughConfident(float3 p) {
				return GDFTex3DAll(p, _AABBmin, _AABBmax, _GlobalDFTex).g;
			}

			float sdf(float3 p){
				float dist = 1000;
				float temp;
				[loop]
				for(int i = 0; i < bufferLength; i++){
					if(GDFInsideAABB(p, _VolumeData[i])){
						temp = GDFAtlasTex3D(p, _VolumeData[i], textureCount, _TextureAtlas);
						dist = min(dist, temp);
					}
				}
				if(dist < 1000) return dist;
				return GDFTex3D(p, _AABBmin, _AABBmax, _GlobalDFTex);
			}

			float2 sdfConfident(float3 p){
				float dist = 1000;
				float temp;
				[loop]
				for(int i = 0; i < bufferLength; i++){
					if(GDFInsideAABB(p, _VolumeData[i])){
						temp = GDFAtlasTex3D(p, _VolumeData[i], textureCount, _TextureAtlas);
						dist = min(dist, temp);
					}
				}
				if(dist < 1000) return float2(dist, dist);
				float t2 = GDFTex3DAll(p, _AABBmin, _AABBmax, _GlobalDFTex);
				return t2;
			}


			float3 getNormalRough(float3 p) {
				const float2 offset = float2(0.001, 0.0);
				float3 normal = float3(
					sdfRough(p + offset.xyy) - sdfRough(p - offset.xyy),
					sdfRough(p + offset.yxy) - sdfRough(p - offset.yxy),
					sdfRough(p + offset.yyx) - sdfRough(p - offset.yyx)
					);
				return normalize(normal);
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

			uniform float _aoStepSize;
			uniform int _aoMaxIter;
			uniform float _aoIntensity;

			float ambientOcclusion(float3 p, float3 n){
				float step = _aoStepSize;
				float ao = 0.0;
				float dist;
				for(int i = 1; i <= _aoMaxIter; i++){
					dist = step * i;
					ao += max((dist - sdfRough(p + n * dist)) / dist, 0.0);
				}
				float angle = max(0, dot(-n, _WorldSpaceLightPos0.xyz));
				return (1.0 - ao * _aoIntensity);
			}

			float _shadowPenumbra;
			float _accuracy;

			float softShadow(float3 pos){
				// currently directional light
				float result = 1.0f;
				float maxDistance = 5.0;
				float step = maxDistance / 10;
				float3 dir = normalize(_WorldSpaceLightPos0.xyz);
				//float3 dir = normalize(_WorldSpaceLightPos0.xyz - pos);
				float t = 0.1;
				[loop]
				while(t < maxDistance){
					//float2 d = sdfConfident(pos + dir * t);
					float d = sdfRough(pos + dir * t);
					if(d < _accuracy){
						return t * t / maxDistance;
					}
					result = min(result, max(t * t / maxDistance, _shadowPenumbra * d / t));

					t += d;
				}
				return result;
			}

			sampler2D _MainTex;
			int _blendWithScene;

			float _shadowIntensity;

            fixed4 frag (v2f i) : SV_Target
            {
				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
				float3 dir = i.ray.xyz;
				float3 pos = _WorldSpaceCameraPos.xyz + dir * depth;
				float3 normal = getNormal(pos);

				float ao = ambientOcclusion(pos, normal);

				float shadow = softShadow(pos);
				shadow = 1 - (1 - shadow) * _shadowIntensity;
				ao = ao * shadow;

				fixed3 col;
				if(_blendWithScene == 1){
					col = tex2D(_MainTex, i.uv) * ao;
				} else {
					col = float3(ao, ao, ao);
				}

                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
