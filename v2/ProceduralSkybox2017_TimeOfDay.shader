Shader "Custom/ProceduralSkybox2017_TimeOfDay"
{
    Properties
    {
        _TimeOfDay ("Time Of Day", Range(0,1)) = 0.5

        _CloudDensity ("Cloud Density", Range(0,1)) = 0.65
        _CloudScale ("Cloud Scale", Range(0.5, 4)) = 1.6
        _CloudSpeed ("Cloud Speed", Range(0, 0.5)) = 0.04
        _CloudSoftness ("Cloud Softness", Range(0.5, 3)) = 1.6

        _HighCloudDensity ("High Cloud Density", Range(0,1)) = 0.35
        _HighCloudScale ("High Cloud Scale", Range(1, 8)) = 3.5
        _HighCloudSpeed ("High Cloud Speed", Range(0, 1)) = 0.12

        _CloudShadowStrength ("Cloud Shadow Strength", Range(0,1)) = 0.4
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

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 viewDir : TEXCOORD0; };

            float _TimeOfDay;
            float _CloudDensity, _CloudScale, _CloudSpeed, _CloudSoftness;
            float _HighCloudDensity, _HighCloudScale, _HighCloudSpeed;
            float _CloudShadowStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(wp - _WorldSpaceCameraPos);
                return o;
            }

            // ---------- NOISE ----------
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float valueNoise(float2 p)
            {
                float2 i=floor(p), f=frac(p);
                float a=hash(i), b=hash(i+float2(1,0));
                float c=hash(i+float2(0,1)), d=hash(i+float2(1,1));
                float2 u=f*f*(3-2*f);
                return lerp(a,b,u.x)+(c-a)*u.y*(1-u.x)+(d-b)*u.x*u.y;
            }
            float fbm(float2 p)
            {
                float v=0,a=0.5;
                for(int i=0;i<4;i++){ v+=valueNoise(p)*a; p*=2; a*=0.5; }
                return v;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // -------- TIME SPLITS --------
                float morning = saturate(1 - abs(_TimeOfDay - 0.0) * 4);
                float midday  = saturate(1 - abs(_TimeOfDay - 0.33) * 4);
                float sunset  = saturate(1 - abs(_TimeOfDay - 0.66) * 4);
                float night   = saturate(1 - abs(_TimeOfDay - 1.0) * 4);

                // -------- SKY COLORS --------
                float3 topColor =
                    morning * float3(0.4,0.6,0.9) +
                    midday  * float3(0.2,0.45,0.9) +
                    sunset  * float3(0.15,0.2,0.45) +
                    night   * float3(0.02,0.04,0.1);

                float3 horizonColor =
                    morning * float3(1.0,0.85,0.6) +
                    midday  * float3(0.85,0.95,1.0) +
                    sunset  * float3(1.0,0.5,0.25) +
                    night   * float3(0.05,0.07,0.15);

                // -------- SUN --------
                float sunHeight =
                    morning * -0.15 +
                    midday  * 0.35 +
                    sunset  * -0.35 +
                    night   * -0.6;

                float3 sunColor =
                    morning * float3(1.0,0.8,0.5) +
                    midday  * float3(1,1,0.95) +
                    sunset  * float3(1.0,0.4,0.2) +
                    night   * float3(0.1,0.1,0.2);

                float sunIntensity =
                    morning * 1.2 +
                    midday  * 1.5 +
                    sunset  * 1.3 +
                    night   * 0.15;

                // -------- SKY GRADIENT --------
                float y = saturate(i.viewDir.y * 0.5 + 0.5);
                float3 sky = lerp(horizonColor, topColor, y);

                // -------- SUN DIR --------
                float3 sunDir = normalize(_WorldSpaceLightPos0.xyz + float3(0,sunHeight,0));
                float sun = pow(saturate(dot(i.viewDir,sunDir)),10) * sunIntensity;

                // -------- CLOUDS --------
                float2 uv = i.viewDir.xz / max(i.viewDir.y + 1.0, 0.3);

                float lowCloud = fbm(uv*_CloudScale + _Time.y*_CloudSpeed);
                lowCloud = smoothstep(_CloudDensity-0.2,_CloudDensity+0.2,lowCloud);
                lowCloud = pow(lowCloud,_CloudSoftness);

                float highCloud = fbm(uv*_HighCloudScale + _Time.y*_HighCloudSpeed);
                highCloud = smoothstep(_HighCloudDensity-0.15,_HighCloudDensity+0.15,highCloud);

                float cloudMask = saturate(lowCloud + highCloud*0.6);

                float shadow = lerp(1,1-cloudMask,_CloudShadowStrength);

                float3 clouds = cloudMask * sunColor * shadow;

                float3 finalColor = sky + clouds + sunColor * sun * (1-cloudMask);

                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
