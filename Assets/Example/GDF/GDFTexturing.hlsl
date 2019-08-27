#ifndef GDF_TEXTURING
#define GDF_TEXTURING

SamplerState gdf_sampler_trilinear_clamp;
SamplerState gdf_sampler_linear_clamp;

struct VolumeData
{
    float4x4 WorldToLocal;
    float3 Extents;
    int TextureIndex;
    float3 Center;
};


inline bool GDFInsideAABB(in float3 pos, in float3 min, in float3 max)
{
    return (pos.x > min.x && pos.y > min.y && pos.z > min.z &&
       pos.x < max.x && pos.y < max.y && pos.z < max.z);
}


// pos center extents
inline bool inside(float3 p, float3 c, float3 e)
{
    float3 i = c - e;
    float3 a = c + e;
    return (p.x > i.x && p.y > i.y && p.z > i.z &&
        p.x < a.x && p.y < a.y && p.z < a.z);
}

inline bool GDFInsideAABB(in float3 pos, in VolumeData data)
{
    float4x4 w2l = data.WorldToLocal;
    float3 extents = data.Extents;
    float3 center = data.Center;

    float3 localPos = mul(w2l, float4(pos, 1)).xyz - center;
    return GDFInsideAABB(localPos, -extents, extents);
}

inline float2 GDFLineAABBIntsc(float3 ro, float3 re, float3 aabbMin, float3 aabbMax)
{
    float3 invRd = 1.0 / (re - ro);
    float3 mini = invRd * (aabbMin - ro);
    float3 maxi = invRd * (aabbMax - ro);
    float3 closest = min(mini, maxi);
    float3 furthest = max(mini, maxi);
    
    float2 intersections;
    //furthest near intersection
    intersections.x = max(closest.x, max(closest.y, closest.z));
    //closest far intersection
    intersections.y = min(furthest.x, min(furthest.y, furthest.z));
    //map 0 ray origin, 1 ray end
    return saturate(intersections);
}


inline float GDFAtlasTex3D(in float3 pos, in VolumeData data, in int numTexture, in Texture3D atlasTex)
{
    float4x4 w2l = data.WorldToLocal;
    float3 extents = data.Extents;
    float index = (float) data.TextureIndex;
    float3 center = data.Center;

    // length of a single texture on z-axis in normalized texture space
    float zunit = 1.0 / numTexture;

    float3 localPos = mul(w2l, float4(pos, 1)).xyz - center;
    float3 uv = localPos / (extents.xyz * 2) + 0.5;
    // scaling the z component of local uv
    uv.z = (index + uv.z) * zunit;

    float sample = atlasTex.Sample(gdf_sampler_linear_clamp, uv).r;
    sample *= length(extents * 2);

    float3 firstRow = w2l._m00_m01_m02;
    float scale = 1.0 / length(firstRow);
    sample *= scale;
    return sample;
}

inline void GDFGlobalDistanceToAABBPlus(in float3 pos, in VolumeData data, in int numTexture, in Texture3D tex, 
    out float aoDist, out float rmDist)
{
    float4x4 w2l = data.WorldToLocal;
    float3 extents = data.Extents;
    float index = (float) data.TextureIndex;
    float3 center = data.Center;

    float zunit = 1.0 / numTexture;

    float3 localPos = mul(w2l, float4(pos, 1)).xyz - center;
    float3 localEnd = float3(0, 0, 0);

    float2 intersection = GDFLineAABBIntsc(localPos, localEnd, - extents, extents);
    
    //float step = saturate(intersection.x + 0.005);
    //float3 lerpPos = lerp(localPos, localEnd, step.xxx);
    
    float3 lerpPos = lerp(localPos, localEnd, intersection.xxx);
    
    float3 uv = lerpPos / (extents.xyz * 2) + 0.5;
    // scaling the z component of local uv
    uv.z = (index + uv.z) * zunit;

    float distInside = tex.Sample(gdf_sampler_linear_clamp, uv).r;
    distInside = max(0, distInside);
    distInside *= length(extents * 2);

    float distOutside = intersection.x * distance(localPos, localEnd);

    //float result = intersection.x * distance(localPos, localEnd) + distInside;
    //float result = max(distInside, distOutside);
    //float result = intersection.x * distance(localPos, localEnd);

    aoDist = distInside + distOutside;
    rmDist = max(distInside, distOutside);

    float3 firstRow = w2l._m00_m01_m02;
    float scale = 1.0 / length(firstRow);
    aoDist *= scale;
    rmDist *= scale;
}

inline float GDFTex3D(in float3 pos, in float3 min, in float3 max, in Texture3D tex)
{
    float3 uvPos = (pos - min) / (max - min);
    uvPos = saturate(uvPos);
    float dist = tex.Sample(gdf_sampler_trilinear_clamp, uvPos).r;

    return dist * length(max - min);
}

inline float2 GDFTex3DAll(in float3 pos, in float3 min, in float3 max, in Texture3D tex)
{
    float3 uvPos = (pos - min) / (max - min);
    uvPos = saturate(uvPos);
    float2 dist = tex.Sample(gdf_sampler_trilinear_clamp, uvPos).rg;

    return dist * length(max - min);
}


#endif