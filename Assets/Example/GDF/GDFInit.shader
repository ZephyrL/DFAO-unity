Shader "GDF/GDFInit"
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
			Name "init"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
			#include "HLSLSupport.cginc"

            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
			#pragma target 3.0

            float2 frag(v2f_init_customrendertexture IN) : COLOR
            {
				//int idx = floor(IN.texcoord.x * _CustomRenderTextureWidth);
				//int idy = floor(IN.texcoord.y * _CustomRenderTextureHeight);
				//int idz = floor(IN.texcoord.z * _CustomRenderTextureDepth);

				//return float4(IN.texcoord, 1.0);

				return float2(0.0, 0.0);
            }
            ENDCG
		}
	}
}
