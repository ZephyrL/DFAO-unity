Shader "GDF/GlobalDistanceFieldClip"
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

            float4 frag(v2f_customrendertexture IN) : SV_TARGET
            {
				return float4(_WorldSpaceCameraPos, 1.0);
            }
            ENDCG
		}
	}
}
