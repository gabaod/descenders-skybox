Shader "Custom/ProceduralSkybox2017_TimeOfDay"
{
    Properties
    {
        _TimeOfDay ("Time Of Day", Range(0,1)) = 0.5
        _SunIntensity ("Sun Intensity", Range(0,3)) = 1.5
        _MoonIntensity ("Moon Intensity", Range(0,1)) = 0.3
        _SunSize ("Sun Size", Range(0.01, 0.2)) = 0.05
        _MoonSize ("Moon Size", Range(0.01, 0.2)) = 0.04

        _CloudDensity ("Cloud Density", Range(0,1)) = 0.65
        _CloudScale ("Cloud Scale", Range(0.5, 4)) = 1.6
        _CloudSpeed ("Cloud Speed", Range(0, 0.5)) = 0.04
        _CloudSoftness ("Cloud Softness", Range(0.5, 3)) = 1.6

        _HighCloudDensity ("High Cloud Density", Range(0,1)) = 0.35
        _HighCloudScale ("High Cloud Scale", Range(1, 8)) = 3.5
        _HighCloudSpeed ("High Cloud Speed", Range(0, 1)) = 0.12

        _CloudShadowStrength ("Cloud Shadow Strength", Range(0,1)) = 0.4
        
        _SmallStarDensity ("Small Star Density", Range(0,1)) = 0.5
        _SmallStarBrightness ("Small Star Brightness", Range(0,2)) = 1.0
        _LargeStarDensity ("Large Star Density", Range(0,1)) = 0.2
        _LargeStarBrightness ("Large Star Brightness", Range(0,2)) = 1.2
        _StarTwinkleSpeed ("Star Twinkle Speed", Range(0,5)) = 1.0
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
            float _SunIntensity, _MoonIntensity;
            float _SunSize, _MoonSize;
            float _CloudDensity, _CloudScale, _CloudSpeed, _CloudSoftness;
            float _HighCloudDensity, _HighCloudScale, _HighCloudSpeed;
            float _CloudShadowStrength;
            float _SmallStarDensity, _SmallStarBrightness;
            float _LargeStarDensity, _LargeStarBrightness;
            float _StarTwinkleSpeed;

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
            float hash3(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
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
            
            // ---------- STAR GENERATION ----------
            float generateStars(float3 dir, float density, float size, float brightness, float time)
            {
                // Create grid of potential star positions
                float3 starPos = dir * 100.0; // Scale up for more star positions
                float3 gridPos = floor(starPos);
                
                // Generate random star at each grid cell
                float starRandom = hash3(gridPos);
                
                // Only show star if random value is above density threshold
                if (starRandom > (1.0 - density))
                {
                    // Calculate distance to center of grid cell
                    float3 cellCenter = gridPos + 0.5;
                    float dist = length(starPos - cellCenter);
                    
                    // Create star point
                    float star = 1.0 - smoothstep(0.0, size, dist);
                    
                    // Add twinkle effect
                    float twinkle = sin(time * _StarTwinkleSpeed + starRandom * 100.0) * 0.3 + 0.7;
                    
                    return star * brightness * twinkle;
                }
                
                return 0.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // -------- FULL DAY CYCLE TIME SPLITS --------
                // 0.0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset, 1.0 = midnight
                
                float midnight1 = saturate(1 - abs(_TimeOfDay - 0.0) * 8);    // Peak at 0.0
                float sunrise   = saturate(1 - abs(_TimeOfDay - 0.25) * 6);   // Peak at 0.25
                float noon      = saturate(1 - abs(_TimeOfDay - 0.5) * 6);    // Peak at 0.5
                float sunset    = saturate(1 - abs(_TimeOfDay - 0.75) * 6);   // Peak at 0.75
                float midnight2 = saturate(1 - abs(_TimeOfDay - 1.0) * 8);    // Peak at 1.0
                
                // Combine both midnight peaks
                float midnight = max(midnight1, midnight2);

                // -------- SKY COLORS --------
                float3 topColor =
                    midnight * float3(0.02,0.04,0.1) +
                    sunrise  * float3(0.4,0.6,0.9) +
                    noon     * float3(0.2,0.45,0.9) +
                    sunset   * float3(0.15,0.2,0.45);

                float3 horizonColor =
                    midnight * float3(0.05,0.07,0.15) +
                    sunrise  * float3(1.0,0.85,0.6) +
                    noon     * float3(0.85,0.95,1.0) +
                    sunset   * float3(1.0,0.5,0.25);

                // -------- SUN COLOR --------
                float3 sunColor =
                    midnight * float3(0.1,0.1,0.2) +
                    sunrise  * float3(1.0,0.8,0.5) +
                    noon     * float3(1,1,0.95) +
                    sunset   * float3(1.0,0.4,0.2);

                // -------- SKY GRADIENT --------
                float y = saturate(i.viewDir.y * 0.5 + 0.5);
                float3 sky = lerp(horizonColor, topColor, y);

                // -------- CALCULATE SUN POSITION --------
                // Full day cycle: sun moves in complete arc
                // 0.0 = midnight (below horizon), 0.25 = sunrise (horizon), 0.5 = noon (overhead), 0.75 = sunset (horizon), 1.0 = midnight
                
                // Offset by -0.25 to align correctly, then multiply by 2PI for full circle
                float sunAngle = (_TimeOfDay - 0.25) * 3.14159 * 2.0; // Shifted to start at horizon
                
                // Calculate sun direction vector
                // sin gives us height (y), cos gives us horizontal position (x)
                float3 sunDir = normalize(float3(cos(sunAngle), sin(sunAngle), 0));
                
                // -------- RENDER SUN DISC --------
                float sunDist = distance(normalize(i.viewDir), sunDir);
                float sunDisc = 1.0 - smoothstep(_SunSize * 0.9, _SunSize, sunDist);
                
                // Only show sun when above horizon
                float sunVisible = step(0, sunDir.y);
                sunDisc *= sunVisible;
                
                // Sun glow
                float sunGlow = pow(saturate(dot(i.viewDir, sunDir)), 15.0) * 0.5 * sunVisible;
                
                // -------- CALCULATE MOON POSITION --------
                // Moon should be opposite the sun in the sky
                // When sun is at 0.25 (horizon), moon should be at 0.75 (opposite horizon)
                // When sun is at 0.5 (overhead), moon should be at 0.0/1.0 (below horizon)
                float moonTimeOffset = _TimeOfDay + 0.5; // Shift by half a day
                if (moonTimeOffset > 1.0) moonTimeOffset -= 1.0; // Wrap around
                
                float moonAngle = (moonTimeOffset - 0.25) * 3.14159 * 2.0;
                float3 moonDir = normalize(float3(cos(moonAngle), sin(moonAngle), 0));
                
                // -------- RENDER MOON DISC --------
                float moonDist = distance(normalize(i.viewDir), moonDir);
                float moonDisc = 1.0 - smoothstep(_MoonSize * 0.9, _MoonSize, moonDist);
                
                // Only show moon when above horizon
                float moonVisible = step(0, moonDir.y);
                moonDisc *= moonVisible;
                
                // Make moon brighter and more visible
                float3 moonColor = float3(0.9, 0.9, 1.0) * 2.0; // Brighter bluish-white moon

                // -------- STARS --------
                // Stars only visible during deep night: 0.0-0.13 and 0.85-1.0
                float nightIntensity = 0.0;
                
                if (_TimeOfDay <= 0.13)
                {
                    // Fade in from 0.0 to 0.13
                    nightIntensity = 1.0 - (_TimeOfDay / 0.13);
                }
                else if (_TimeOfDay >= 0.85)
                {
                    // Fade in from 0.85 to 1.0
                    nightIntensity = (_TimeOfDay - 0.85) / 0.15;
                }
                
                float3 stars = float3(0,0,0);
                if (nightIntensity > 0.01 && i.viewDir.y > 0) // Only render stars at night and above horizon
                {
                    // Small stars (more numerous, dimmer)
                    float smallStars = generateStars(i.viewDir, _SmallStarDensity, 0.15, _SmallStarBrightness, _Time.y);
                    
                    // Large stars (fewer, brighter)
                    float largeStars = generateStars(i.viewDir, _LargeStarDensity, 0.25, _LargeStarBrightness, _Time.y * 0.7);
                    
                    // Combine both star layers
                    float totalStars = smallStars + largeStars;
                    
                    // Star color (slightly bluish white)
                    stars = float3(0.9, 0.95, 1.0) * totalStars * nightIntensity;
                }

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

                // -------- COMBINE EVERYTHING --------
                float3 finalColor = sky;
                finalColor += stars; // Add stars behind clouds
                finalColor += clouds;
                finalColor += sunColor * sunDisc * _SunIntensity;
                finalColor += sunColor * sunGlow * _SunIntensity;
                finalColor += moonColor * moonDisc * _MoonIntensity * 5.0;

                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
