Shader "Custom/Turbulence"
{
    Properties
    {
        // ── Existing material properties ──
        _FlowSpeed      ("Flow Speed",      Range(0, 2))        = 0.4
        _OverallAlpha   ("Overall Alpha",   Range(0, 1))        = 0.6
        _StripeCount    ("Stripe Count",    Range(1, 20))       = 4
        _StripeWidth    ("Stripe Width",    Range(0.05, 0.95))  = 0.5
        _Turbulence     ("Turbulence",      Range(0, 1))        = 0.6
        _WaveAmp        ("Wave Amplitude",  Range(0, 0.3))      = 0.08
        _BrightColor    ("Bright Color",    Color)              = (0.2, 0.8, 1.0, 0.55)
        _DarkColor      ("Dark Color",      Color)              = (0.05, 0.3, 0.6, 0.25)
        _FlowDirection  ("Flow Direction",  Vector)             = (0, 1, 0, 0)

        // ── New properties ──
        _SlantAmount    ("Slant",           Range(-0.5, 0.5))   = 0.15
        _NoiseScale     ("Noise Scale",     Range(0.5, 8))      = 2.0
        _NoiseSpeed     ("Noise Speed",     Range(0, 1))        = 0.3
        [Toggle] _EdgeFade ("Edge Fade",    Float)              = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Turbulence"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Material properties ──
            CBUFFER_START(UnityPerMaterial)
                float _FlowSpeed;
                float _OverallAlpha;
                float _StripeCount;
                float _StripeWidth;
                float _Turbulence;
                float _WaveAmp;
                float4 _BrightColor;
                float4 _DarkColor;
                float4 _FlowDirection;
                float _SlantAmount;
                float _NoiseScale;
                float _NoiseSpeed;
                float _EdgeFade;
            CBUFFER_END

            // ── Vertex I/O ──
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

            // ── Hash & noise ──
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(hash(i), hash(i + float2(1, 0)), f.x),
                    lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x),
                    f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amp   = 0.5;
                float freq  = 1.0;
                for (int i = 0; i < 3; i++)
                {
                    value += amp * noise2D(p * freq);
                    freq  *= 2.0;
                    amp   *= 0.5;
                }
                return value;
            }

            // ── Vertex shader ──
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv         = input.uv;
                output.color      = input.color;
                return output;
            }

            // ── Fragment shader ──
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv   = input.uv;
                float2 flow = normalize(_FlowDirection.xy + 1e-5);
                float2 perp = float2(-flow.y, flow.x);

                // Project UV along & across flow
                float along  = dot(uv, flow);
                float across = dot(uv, perp);

                // ── Turbulence noise ──
                float2 noiseUV = uv * _NoiseScale + _Time.y * _NoiseSpeed;
                float  turb    = (fbm(noiseUV) - 0.5) * _Turbulence;

                // ── Scroll along flow ──
                along += _Time.y * _FlowSpeed;

                // ── Slanted stripe coordinate ──
                float slant   = across * _SlantAmount;
                float waveFreq = _StripeCount * 3.14159 * 2.0;
                float wave    = sin(across * waveFreq + _Time.y) * _WaveAmp;
                float stripeCoord = (along + slant + wave + turb) * _StripeCount;

                // ── Triangle wave → stripe mask ──
                float tri     = 1.0 - abs(frac(stripeCoord) * 2.0 - 1.0);
                float edge    = 1.0 - _StripeWidth;
                float stripe  = smoothstep(edge - 0.03, edge + 0.03, tri);

                // ── Color blend ──
                float4 color = lerp(_DarkColor, _BrightColor, stripe);

                // ── Alpha with edge fade ──
                float edgeAlpha = lerp(1.0, input.color.a, _EdgeFade);
                color.a *= edgeAlpha * _OverallAlpha;

                return color;
            }
            ENDHLSL
        }
    }
}
