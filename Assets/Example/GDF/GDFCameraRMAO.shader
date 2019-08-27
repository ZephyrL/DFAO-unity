Shader "GDF/GDFCameraRMAO"
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
			#pragma target 3.0

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

            sampler2D _MainTex;
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
			uniform int bufferLength;
			Texture3D _TextureAtlas;	// local SDF's
			int textureCount;
			Texture3D _GlobalDFTex;		// gloabl SDF
			
			float sdfRough(float3 p){
				return GDFTex3D(p, _AABBmin, _AABBmax, _GlobalDFTex);
			}

			float sdf(float3 p){
				if(!GDFInsideAABB(p, _AABBmin, _AABBmax)){
					return 0.1;
				}
				float dist = 1e4;
				float temp;
				
				[loop] for(int i = 0; i < bufferLength; i++){
					if(GDFInsideAABB(p, _VolumeData[i])){
						temp = GDFAtlasTex3D(p, _VolumeData[i], textureCount, _TextureAtlas);
						dist = min(dist, temp);
					}
				}
				if(dist < 1e4) return dist;
				temp = GDFTex3D(p, _AABBmin, _AABBmax, _GlobalDFTex);
				dist = min(dist, temp);
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

			uniform float _aoStepSize;
			uniform int _aoMaxIter;
			uniform float _aoIntensity;

			float ambientOcclusion(float3 p, float3 n){
				float step = _aoStepSize;
				float ao = 0.0;
				float dist;
				
				[loop]for(int i = 1; i <= _aoMaxIter; i++){
					dist = step * i;
					ao += max((dist - sdfRough(p + n * dist)) / dist, 0.0);
				}
				return (1.0 - ao * _aoIntensity);
			}

			float3 shading(float3 p, float3 n){
				// direction light
				float3 light = (dot(_WorldSpaceLightPos0, n) * 0.5 + 0.5);
				// shadow
				//float shadow = hardShadow(p, -_lightDir, _shadowDistance.x, _shadowDistance.y) * 0.5 + 0.5;
				//float shadow = softShadow(p, -_lightDir, _shadowDistance.x, _shadowDistance.y, _shadowPenumbra) * 0.5 + 0.5;
				//shadow = max(0.0, pow(shadow, _shadowIntensity));
				// AO
				float ao = ambientOcclusion(p, n);

				//return light * ao;
				return float3(ao, ao, ao) ;
			}

			uniform int _maxIteration;
			uniform float _accuracy;

			fixed4 raymarching(float3 ro, float3 rd, float depth) {
				fixed4 result = fixed4(1, 1, 1, 1);
				
				const int max_iter = _maxIteration; // max step of marching
				const float maxDistance = 1000;

				float t = 0; // distance travelled 

				[loop]for (int i = 0; i < max_iter; i++) {
					if (t > maxDistance || t >= depth * 2) {
						// not hit
						result = fixed4(rd, 1);
						break;
					}

					float3 p = ro + rd * t;
					// check hit
					float d = sdf(p);
					//if(d < 0) return fixed4(0.2, 0.5, 0.8, 1);
					if (d <= _accuracy) {
						// hit
						if(t > depth) p = ro + rd * depth;
						float3 n = getNormal(p);
						float3 s = shading(p, n);
						//float steps = i / (float)max_iter;
						result = fixed4(s, 1);
						//result = fixed4(0.2, 0.5, 0.8, 1);
						break;
					}

					t += d;
					//if(i == max_iter - 1){
					//	float c = t / depth;
					//	result = fixed4(c,c,c,1);
					//}
				}

				return result;
			}

			int _blendWithScene;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 color = tex2D(_MainTex, i.uv);
				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
				depth *= length(i.ray);
 				float3 rayDir = normalize(i.ray.xyz);
				float3 rayOri = _WorldSpaceCameraPos;
				fixed4 result = raymarching(rayOri, rayDir, depth);
				if(_blendWithScene == 1){
					return fixed4(result.xyz * color, 1.0);
				}
				else{
					return fixed4(result.xyz, 1.0);
				}
				//return fixed4(color * (1 - result.w) + result.xyz * result.w ,1.0);
            }
            ENDCG
        }
    }
}
