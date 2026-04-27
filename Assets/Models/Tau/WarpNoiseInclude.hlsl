#ifndef DOMAIN_WARP_INCLUDED
#define DOMAIN_WARP_INCLUDED

// Domain warp using a pre-baked noise texture (RG channels = warp offset, BA = fBm detail).
void DomainWarpFast_float(
    float2 UV,
    float WarpStrength,
    float Time,
    UnityTexture2D NoiseTex,
    UnitySamplerState NoiseTexSampler,
    out float Out)
{
    // Uv warping
    float2 q = SAMPLE_TEXTURE2D(NoiseTex, NoiseTexSampler, UV * 0.5 + Time * 0.05).rg * 2.0 - 1.0;
    float2 warped = UV + WarpStrength * q;
    
    // Fetch fbm noise at multiple frequencies
    float2 s0 = SAMPLE_TEXTURE2D(NoiseTex, NoiseTexSampler, warped).ba;
    float2 s1 = SAMPLE_TEXTURE2D(NoiseTex, NoiseTexSampler, warped * 2.03).ba;
    float2 s2 = SAMPLE_TEXTURE2D(NoiseTex, NoiseTexSampler, warped * 4.07).ba;
    
    float v = s0.x * 0.50 + s0.y * 0.25
            + s1.x * 0.125 + s1.y * 0.0625
            + s2.x * 0.03125 + s2.y * 0.015625;
    
    Out = v * 2.0 - 1.0;
}

#endif // DOMAIN_WARP_INCLUDED
