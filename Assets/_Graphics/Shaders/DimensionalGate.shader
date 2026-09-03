Shader "Custom/2D/DimensionalGate"
{
    Properties
    {
        [Header(Color)]
        [HDR] _CoreColor  ("Core Color", Color)  = (0.18, 0.04, 0.45, 1)
        [HDR] _EdgeColor  ("Edge Color", Color)  = (0.35, 1.60, 1.90, 1)
        [HDR] _TrailColor ("Trail Color", Color) = (1.60, 0.90, 2.40, 1)

        [Header(Shape)]
        _CapsuleWidth  ("Capsule Width",  Range(0.10, 1.00)) = 0.55
        _Softness      ("Edge Softness",  Range(0.001, 0.50)) = 0.08
        _EdgeThickness ("Edge Thickness", Range(0.50, 8.00)) = 3.0
        _RimStart      ("Rim Start",      Range(0.00, 1.00)) = 0.55

        [Header(Swirl)]
        _SwirlStrength ("Swirl Strength", Range(0.0, 10.0)) = 3.0
        _SwirlSpeed    ("Swirl Speed",    Range(-5.0, 5.0)) = 1.2
        _NoiseScale    ("Noise Scale",    Range(1.0, 20.0)) = 6.0
        _FlowSpeed     ("Flow Speed",     Range(0.0, 5.0))  = 0.8

        [Header(Rim Trail)]
        _TrailCount     ("Trail Count",     Range(1, 8))      = 2
        _TrailSpeed     ("Trail Speed",     Range(-4.0, 4.0)) = 0.6
        _TrailTail      ("Trail Tail",      Range(1.0, 24.0)) = 6.0
        _TrailIntensity ("Trail Intensity", Range(0.0, 6.0))  = 2.2

        [Header(Rings)]
        _RingCount     ("Ring Count",     Range(0.0, 12.0)) = 4.0
        _RingSpeed     ("Ring Speed",     Range(-8.0, 8.0)) = 3.0
        _RingSharp     ("Ring Sharpness", Range(1.0, 24.0)) = 8.0
        _RingIntensity ("Ring Intensity", Range(0.0, 4.0))  = 0.8

        [Header(Sparkle)]
        _SparkleDensity   ("Sparkle Density",   Range(2.0, 40.0)) = 14.0
        _SparkleSharp     ("Sparkle Sharpness", Range(2.0, 60.0)) = 24.0
        _SparkleIntensity ("Sparkle Intensity", Range(0.0, 4.0))  = 1.2

        [Header(General)]
        _PulseSpeed ("Pulse Speed", Range(0.0, 8.0)) = 2.0
        _PulseDepth ("Pulse Depth", Range(0.0, 0.6)) = 0.15
        _PixelSize  ("Pixel Size (0=off)", Range(0, 128)) = 48
        _Alpha      ("Overall Alpha", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"  = "UniversalPipeline"
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define TWO_PI 6.28318530718

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CoreColor;
                float4 _EdgeColor;
                float4 _TrailColor;
                float  _CapsuleWidth;
                float  _Softness;
                float  _EdgeThickness;
                float  _RimStart;
                float  _SwirlStrength;
                float  _SwirlSpeed;
                float  _NoiseScale;
                float  _FlowSpeed;
                float  _TrailCount;
                float  _TrailSpeed;
                float  _TrailTail;
                float  _TrailIntensity;
                float  _RingCount;
                float  _RingSpeed;
                float  _RingSharp;
                float  _RingIntensity;
                float  _SparkleDensity;
                float  _SparkleSharp;
                float  _SparkleIntensity;
                float  _PulseSpeed;
                float  _PulseDepth;
                float  _PixelSize;
                float  _Alpha;
            CBUFFER_END

            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i + float2(0.0, 0.0));
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv         = IN.uv;
                OUT.color      = IN.color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;

                float2 uv = IN.uv;
                if (_PixelSize > 0.5)
                    uv = (floor(uv * _PixelSize) + 0.5) / _PixelSize;

                float2 p = (uv - 0.5) * 2.0;

                // 가로를 눌러 세로로 긴 타원(캡슐)로. 이 공간에서 모든 계산을 한다.
                float2 shaped = float2(p.x / max(_CapsuleWidth, 0.01), p.y);
                float  dist   = length(shaped);

                float mask = 1.0 - smoothstep(1.0 - _Softness, 1.0, dist);

                // 테두리 부근만 1에 가까워지는 값. 트레일과 링을 여기에 가둔다.
                float rim = smoothstep(_RimStart, 1.0, dist) * mask;

                // ---- 안쪽 소용돌이 ----
                float  radius = length(p);
                float  angle  = atan2(p.y, p.x)
                              + _SwirlStrength * (1.0 - saturate(radius))
                              + t * _SwirlSpeed;
                float2 swirlUV = float2(cos(angle), sin(angle)) * radius;

                float n = ValueNoise(swirlUV * _NoiseScale + float2(0.0, t * _FlowSpeed));
                n += ValueNoise(swirlUV * _NoiseScale * 2.0 - float2(t * _FlowSpeed * 0.7, 0.0)) * 0.5;
                n /= 1.5;

                // ---- 테두리를 도는 트레일 ----
                // 각도를 0~1로 편 뒤 시간만큼 밀면 frac 경계가 테두리를 따라 돈다.
                float a01   = atan2(shaped.y, shaped.x) / TWO_PI + 0.5;
                float head  = frac(a01 * _TrailCount + t * _TrailSpeed);
                float comet = pow(saturate(1.0 - head), _TrailTail);
                float trail = comet * rim;

                // ---- 밖으로 퍼지는 링 ----
                float ring = sin(dist * _RingCount * TWO_PI - t * _RingSpeed) * 0.5 + 0.5;
                ring = pow(ring, _RingSharp) * rim;

                // ---- 반짝이 ----
                float2 cell    = floor(swirlUV * _SparkleDensity);
                float  h       = Hash21(cell);
                float  twinkle = sin(t * (1.5 + h * 5.0) + h * 31.4) * 0.5 + 0.5;
                float  sparkle = pow(twinkle, _SparkleSharp) * step(0.82, h) * mask;

                // ---- 합치기 ----
                float pulse = 1.0 + sin(t * _PulseSpeed) * _PulseDepth;

                float  edge = pow(saturate(dist), _EdgeThickness);
                float3 col  = lerp(_CoreColor.rgb, _EdgeColor.rgb, edge);
                col *= 0.60 + n * 0.80;

                col += _TrailColor.rgb * trail   * _TrailIntensity * pulse;
                col += _EdgeColor.rgb  * ring    * _RingIntensity;
                col += _TrailColor.rgb * sparkle * _SparkleIntensity;

                float alpha = mask * (0.55 + n * 0.45);
                alpha = saturate(alpha + trail * 0.9 + ring * 0.5 + sparkle);
                alpha *= _Alpha;

                return float4(col, alpha) * IN.color;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
