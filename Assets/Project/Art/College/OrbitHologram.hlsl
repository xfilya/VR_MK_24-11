#include "ShaderApiReflectionSupport.hlsl"
#ifndef ORBIT_MASCOT_HOLOGRAM_INCLUDED
#define ORBIT_MASCOT_HOLOGRAM_INCLUDED

float OrbitPortraitMask(float2 uv)
{
    float2 inset = min(uv, 1.0 - uv);
    return smoothstep(0.0, 0.013, min(inset.x, inset.y)) * smoothstep(0.0, 0.095, uv.y);
}

/// <funchints>
///     <sg:ProviderKey>Orbit.College.Hologram</sg:ProviderKey>
///     <sg:DisplayName>Mascot Hologram</sg:DisplayName>
///     <sg:SearchCategory>Orbit/Projection</sg:SearchCategory>
///     <sg:SearchTerms>Hologram, Mascot, Scanlines, Projection</sg:SearchTerms>
/// </funchints>
/// <paramhints name="UV">
///     <UV/>
///     <Default>UV0</Default>
/// </paramhints>
UNITY_EXPORT_REFLECTION
void OrbitMascotHologram(UnityTexture2D Portrait, float2 UV, float Seconds, out float3 Light, out float Opacity)
{
    float2 uv = (UV - float2(0.06, 0.185)) / float2(0.88, 0.79);
    float4 portraitSample = Portrait.Sample(Portrait.samplerstate, saturate(uv));
    float mask = portraitSample.a * OrbitPortraitMask(uv);

    // Preserve facial contrast while mapping the orange coat and white bib to different shades of light.
    float tone = sqrt(saturate(dot(portraitSample.rgb, float3(0.32, 0.56, 0.12))));
    float3 ink = float3(0.009, 0.055, 0.15);
    float3 blue = lerp(ink, float3(0.045, 0.54, 1.05), smoothstep(0.06, 0.68, tone));
    blue = lerp(blue, float3(0.63, 1.16, 1.32), smoothstep(0.62, 1.0, tone));

    // Filter the scanlines as they become smaller than a screen pixel.
    float rows = UV.y * 86.0;
    float visibleRows = 1.0 - smoothstep(0.20, 0.58, fwidth(rows));
    float scan = cos(6.2831853 * (rows - Seconds * 0.52)) * visibleRows;
    float sweepDistance = abs(frac(UV.y - Seconds * 0.145) - 0.5);
    float sweep = 1.0 - smoothstep(0.012, 0.060, sweepDistance);
    blue *= 1.0 + 0.18 * scan + 0.19 * sweep;

    float2 edgeStep = float2(0.004, 0);
    float around = max(max(Portrait.Sample(Portrait.samplerstate, saturate(uv + edgeStep)).a,
                           Portrait.Sample(Portrait.samplerstate, saturate(uv - edgeStep)).a),
                       max(Portrait.Sample(Portrait.samplerstate, saturate(uv + edgeStep.yx)).a,
                           Portrait.Sample(Portrait.samplerstate, saturate(uv - edgeStep.yx)).a));
    float rim = saturate(around - portraitSample.a) * OrbitPortraitMask(uv);

    // A soft cone ties the floating portrait to its physical emitter.
    float width = lerp(0.065, 0.45, smoothstep(0.0, 0.75, UV.y));
    float radius = abs(UV.x - 0.50) / width;
    float beam = (1.0 - smoothstep(0.66, 1.03, radius)) * exp2(-8.5 * UV.y);
    beam *= smoothstep(0.0, 0.025, UV.y) * (0.75 + 0.25 * cos(radius * 29.0));
    float body = mask * (0.87 + 0.06 * scan);
    float glow = saturate(rim * 0.68 + beam * 0.45) * (1.0 - body);
    Opacity = saturate(body + glow);
    Light = (blue * body + float3(0.08, 0.80, 1.45) * glow) / max(Opacity, 0.001);
}
#endif
