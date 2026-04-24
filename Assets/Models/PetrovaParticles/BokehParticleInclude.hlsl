// Cat's-eye / lemon bokeh: circular bokeh squashes toward a lemon shape
// the closer to the screen edge the particle sits.
void CatsEyeBokeh_float(
    float2   UV,
    float2   ScreenPos,       // 0-1 range (Shader Graph Mode: Default)
    float    SquashStrength,
    float    LemonStrength,
    float    LemonSoftness,
    float    EdgeFalloff,
    out float2 CatsEyeUV,
    out float  Mask)
{
    float2 radial    = ScreenPos - 0.5;
    float  dist      = length(radial);
    float2 direction = dist > 1e-5 ? radial / dist : float2(1, 0);

    float2 uv_c = UV - 0.5;
    float2 uv_r = float2(
         uv_c.x * direction.x + uv_c.y * direction.y,
        -uv_c.x * direction.y + uv_c.y * direction.x
    );

    float squash = max(1.0 - dist * SquashStrength, 0.1);
    CatsEyeUV = saturate(float2(uv_r.x / squash, uv_r.y) + 0.5);

    // Lemon mask: intersection of two circles offset along the radial axis.
    // Sharing dy² avoids computing uv_r.y² twice.
    float lemonOffset = dist * LemonStrength;
    float dy2         = uv_r.y * uv_r.y;
    float dxA         = uv_r.x - lemonOffset;
    float dxB         = uv_r.x + lemonOffset;
    float distA       = sqrt(dxA * dxA + dy2);
    float distB       = sqrt(dxB * dxB + dy2);

    // Radius sized so both circles pass through (±0.5, 0)
    float radius = sqrt(0.25 + lemonOffset * lemonOffset);
    float maskA  = 1.0 - smoothstep(radius - LemonSoftness, radius, distA);
    float maskB  = 1.0 - smoothstep(radius - LemonSoftness, radius, distB);

    // saturate prevents negative mask when dist > 1 / (2 * EdgeFalloff)
    float falloff = saturate(1.0 - dist * 2.0 * EdgeFalloff);
    Mask = maskA * maskB * falloff;
}
