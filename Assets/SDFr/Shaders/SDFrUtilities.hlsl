#ifndef XRA_RAYMARCH_UTILITIES
#define XRA_RAYMARCH_UTILITIES

float3 HsvToRgb(float3 c)
{
	const float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float map(float value, float istart, float istop, float ostart, float ostop)
{
	float perc = (value - istart) / (istop - istart);
	value = perc * (ostop - ostart) + ostart;
	return value;
}

float sdSphere(float3 p, float s)
{
    return length(p) - s;
}
            
float sdBox(float3 p, float3 b)
{
    float3 d = abs(p) - b;
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

#endif