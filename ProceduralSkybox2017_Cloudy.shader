Shader "Custom/ProceduralSkybox2017_Cloudy"
{
    Properties
    {
        // Sky
        _TopColor ("Top Color", Color) = (0.25, 0.45, 0.8, 1)
        _HorizonColor ("Horizon Color", Color) = (0.9, 0.95, 1, 1)
        _BottomColor ("Bottom Color", Color) = (0.95, 0.95, 0.95, 1)
        _HorizonPower ("Horizon Falloff", Range(0.1, 5)) = 1.5

        // Sun
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.85, 1)
        _SunSize ("Sun Size", Range(0.02, 0.25)) = 0.12
        _SunIntensity ("Sun Intensity", Range(0, 5)) = 1.5

        // Clouds
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _CloudDensity ("Cloud Density", Range(0,1)) = 0.65
        _CloudScale ("Cloud Scale", Range(0.5, 4)) = 1.8
        _CloudSpeed ("Cloud Speed", Range(0, 0.5)) = 0.05
        _CloudSoftness ("Cloud Softness", Range(0.5, 3)) = 1.5
        _CloudBrightness ("Cloud Brightness", Range(0,2)) = 1.2
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _TopColor, _HorizonColor, _BottomColor;
            float _HorizonPower;

            float4 _SunColor;
            float _SunSize, _SunIntensity;

            float4 _CloudColor;
            float _CloudDensity, _CloudScale, _CloudSpeed;
            float _CloudSoftness, _CloudBrightness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(worldPos - _WorldSpaceCameraPos);
                return o;
            }

            // -------- SMOOTH VALUE NOISE --------
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;

                for (int i = 0; i < 4; i++)
                {
                    v += valueNoise(p) * a;
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float y = saturate(i.viewDir.y * 0.5 + 0.5);

                // SKY
                float horizonBlend = pow(y, _HorizonPower);
                float3 sky = lerp(_BottomColor.rgb, _HorizonColor.rgb, horizonBlend);
                sky = lerp(sky, _TopColor.rgb, y);

                // SUN
                float3 sunDir = normalize(_WorldSpaceLightPos0.xyz);
                float sunDot = saturate(dot(i.viewDir, sunDir));
                float sun = pow(sunDot, 1.0 / _SunSize) * _SunIntensity;

                // CLOUDS
                float2 cloudUV = i.viewDir.xz / max(i.viewDir.y + 1.0, 0.3);
                cloudUV *= _CloudScale;
                cloudUV += _Time.y * _CloudSpeed;

                float cloudNoise = fbm(cloudUV);
                cloudNoise = smoothstep(
                    _CloudDensity - 0.2,
                    _CloudDensity + 0.2,
                    cloudNoise
                );

                cloudNoise = pow(cloudNoise, _CloudSoftness);

                // Sun diffused by clouds
                float sunThroughClouds = sun * (1.0 - cloudNoise);

                float3 clouds =
                    _CloudColor.rgb *
                    cloudNoise *
                    _CloudBrightness;

                clouds += _SunColor.rgb * sunThroughClouds;

                float3 finalColor = sky + clouds;

                return fixed4(finalColor, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}
