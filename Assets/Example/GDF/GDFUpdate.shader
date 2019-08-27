Shader "GDF/GDFUpdate"
{
    Properties
	{
    }

	SubShader
    {
		Lighting Off
        Blend One Zero

        Pass
        {
			Name "Update"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
			#include "HLSLSupport.cginc"

			#include "Assets/Example/GDF/GDFTexturing.hlsl"

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
			#pragma target 4.0

			Texture3D _TextureAtlas;
			StructuredBuffer<VolumeData> _VolumeData;
			int bufferLength;
			int textureCount;

			float3 _AABBmin, _AABBmax;

            float2 frag(v2f_customrendertexture IN) : SV_TARGET
            {
				float3 size = _AABBmax - _AABBmin;
				float3 center = (_AABBmin + _AABBmax ) * 0.5;

				float3 globalPos = IN.globalTexcoord * size + _AABBmin;
				
				const float LARGE_DISTANCE = 1e9;

				float distance1 = LARGE_DISTANCE;
				float distance2 = LARGE_DISTANCE;
				//bool isInsideAABB = false;
				//bool isExact = false;

				float temp = 0;
				float tempC = 0;

				for(int i = 0; i < bufferLength; i++){
					if(GDFInsideAABB(globalPos, _VolumeData[i])){
						temp = GDFAtlasTex3D(globalPos, _VolumeData[i], textureCount, _TextureAtlas);
						distance1 = min(distance1, temp);
						distance2 = min(distance2, temp);
					} else {
						GDFGlobalDistanceToAABBPlus(globalPos, _VolumeData[i], textureCount, _TextureAtlas, temp, tempC);
						distance1 = min(distance1, temp);	// minimun among enlarged ao distances;
						distance2 = min(distance2, tempC);	// mininum between Confidence march step and true distances
					}
				}

				distance1 = distance1 / length(size);
				distance2 = distance2 / length(size);

				//return fixed4(distance1, 0, (float)isInsideAABB, float(isExact));
				return float2(distance1, distance2);
            }
            ENDCG
		}
	}
}

//float distance = 0;
//float minDistance = 1000;
//if(InsideAABB(globalPos, _VolumeBuffer[0])){
//	distance = DistanceFunctionTex3DFast(globalPos, _VolumeBuffer[0], _VolumeATex);
//} else {
//	distance = GlobalDistanceToAABB(globalPos, _VolumeBuffer[0]) + 0.02;
//}
//minDistance = min(distance, minDistance);

//if(InsideAABB(globalPos, _VolumeBuffer[1])){
//	distance = DistanceFunctionTex3DFast(globalPos, _VolumeBuffer[1], _VolumeBTex);
//} else {
//	distance = GlobalDistanceToAABB(globalPos, _VolumeBuffer[1]) + 0.02;
//}
//minDistance = min(distance, minDistance);

//if(InsideAABB(globalPos, _VolumeBuffer[4])){
//	distance = DistanceFunctionTex3DFast(globalPos, _VolumeBuffer[4], _VolumeCTex);
//} else {
//	distance = GlobalDistanceToAABB(globalPos, _VolumeBuffer[4]) + 0.02;
//}
//minDistance = min(distance, minDistance);

//float sphere = sdSphere(globalPos - _Sphere.xyz, _Sphere.w);
//            float box = sdBox(globalPos - _Box.xyz, float3(1, 1, 1) * _Box.w);

//minDistance = min(minDistance, sphere);
//minDistance = min(minDistance, box);

//minDistance = minDistance / length(size);

//return fixed4(minDistance, minDistance, minDistance, 1);