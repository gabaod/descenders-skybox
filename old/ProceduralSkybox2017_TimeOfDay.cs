Shader "Custom/ProceduralSkybox2017_TimeOfDay"
{
    Properties
    {
        _TimeOfDay ("Time Of Day", Range(0,1)) = 0.5
        _SunIntensity ("Sun Intensity", Range(0,3)) = 1.5
        _MoonIntensity ("Moon Intensity", Range(0,1)) = 0.3
        _SunSize ("Sun Size", Range(0.01, 0.2)) = 0.05
        _MoonSize ("Moon Size", Range(0.01, 0.2)) = 0.04
        _MoonRotationOffset ("Moon Rotation Offset", Float) = 180
        _SunHaloSize ("Sun Halo Size", Range(0.1, 1.0)) = 0.3
        _SunHaloIntensity ("Sun Halo Intensity", Range(0,2)) = 0.5
        _MoonHaloSize ("Moon Halo Size", Range(0.1, 1.0)) = 0.25
        _MoonHaloIntensity ("Moon Halo Intensity", Range(0,2)) = 0.3

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
        
        // Storm System
        _Storm1Active ("Storm 1 Active", Float) = 0
        _Storm1Position ("Storm 1 Position", Vector) = (0,0,0,0)
        _Storm1Radius ("Storm 1 Radius", Float) = 50
        _Storm1Intensity ("Storm 1 Intensity", Range(0,1)) = 0.5
        _Storm1Speed ("Storm 1 Speed", Float) = 0.1
        _Storm1Coverage ("Storm 1 Coverage", Range(0,1)) = 0.7
        
        _Storm2Active ("Storm 2 Active", Float) = 0
        _Storm2Position ("Storm 2 Position", Vector) = (0,0,0,0)
        _Storm2Radius ("Storm 2 Radius", Float) = 50
        _Storm2Intensity ("Storm 2 Intensity", Range(0,1)) = 0.5
        _Storm2Speed ("Storm 2 Speed", Float) = 0.1
        _Storm2Coverage ("Storm 2 Coverage", Range(0,1)) = 0.7
        
        _Storm3Active ("Storm 3 Active", Float) = 0
        _Storm3Position ("Storm 3 Position", Vector) = (0,0,0,0)
        _Storm3Radius ("Storm 3 Radius", Float) = 50
        _Storm3Intensity ("Storm 3 Intensity", Range(0,1)) = 0.5
        _Storm3Speed ("Storm 3 Speed", Float) = 0.1
        _Storm3Coverage ("Storm 3 Coverage", Range(0,1)) = 0.7
        
        _StormCloudDarkness ("Storm Cloud Darkness", Color) = (0.2, 0.2, 0.25, 1)
        _EnvironmentDarkening ("Environment Darkening", Range(0,1)) = 0.5
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
            float _MoonRotationOffset;
            float _SunHaloSize, _SunHaloIntensity;
            float _MoonHaloSize, _MoonHaloIntensity;
            float _CloudDensity, _CloudScale, _CloudSpeed, _CloudSoftness;
            float _HighCloudDensity, _HighCloudScale, _HighCloudSpeed;
            float _CloudShadowStrength;
            float _SmallStarDensity, _SmallStarBrightness;
            float _LargeStarDensity, _LargeStarBrightness;
            float _StarTwinkleSpeed;
            
            // Storm variables
            float _Storm1Active, _Storm2Active, _Storm3Active;
            float4 _Storm1Position, _Storm2Position, _Storm3Position;
            float _Storm1Radius, _Storm2Radius, _Storm3Radius;
            float _Storm1Intensity, _Storm2Intensity, _Storm3Intensity;
            float _Storm1Speed, _Storm2Speed, _Storm3Speed;
            float _Storm1Coverage, _Storm2Coverage, _Storm3Coverage;
            float4 _StormCloudDarkness;
            float _EnvironmentDarkening;

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
                float3 starPos = dir * 100.0;
                float3 gridPos = floor(starPos);
                
                // Generate multiple random values for more variation
                float starRandom = hash3(gridPos);
                float starRandom2 = hash3(gridPos + float3(123.456, 789.012, 345.678));
                float starRandom3 = hash3(gridPos * 2.0 + float3(987.654, 321.098, 765.432));
                
                // Only show star if random value is above density threshold
                if (starRandom > (1.0 - density))
                {
                    // Add random offset to star position within cell
                    float3 randomOffset = float3(
                        starRandom2 - 0.5,
                        starRandom3 - 0.5,
                        (starRandom + starRandom2) * 0.5 - 0.5
                    ) * 0.8;
                    
                    float3 cellCenter = gridPos + 0.5 + randomOffset;
                    float dist = length(starPos - cellCenter);
                    
                    // Vary star size slightly
                    float sizeVariation = 0.7 + (starRandom2 * 0.6);
                    float star = 1.0 - smoothstep(0.0, size * sizeVariation, dist);
                    
                    // Add twinkle effect with varied speeds per star
                    float twinkleSpeed = 0.5 + (starRandom3 * 1.5);
                    float twinkle = sin(time * _StarTwinkleSpeed * twinkleSpeed + starRandom * 100.0) * 0.4 + 0.6;
                    
                    // Vary brightness per star
                    float brightnessVariation = 0.5 + (starRandom * 0.5);
                    
                    return star * brightness * brightnessVariation * twinkle;
                }
                
                return 0.0;
            }
            
            // ---------- STORM CLOUD GENERATION ----------
            float generateStorm(float3 dir, float2 stormPos, float radius, float intensity, float speed, float coverage, float time)
            {
                // Project view direction onto XZ plane for storm position
                float2 skyPos = dir.xz / max(dir.y + 0.5, 0.2);
				
                // Calculate distance from storm center
                float dist = length(skyPos - stormPos);
                
                // Storm influence falls off with distance
                float stormMask = 1.0 - smoothstep(radius * 0.0, radius, dist);
                
                if (stormMask > 0.01)
                {
                    // Generate dense, dark storm clouds
                    float2 stormUV = skyPos * 2.0 + time * speed;
                    float stormCloud = fbm(stormUV);
                    
                    // Make clouds denser and more dramatic
                    stormCloud = smoothstep(1.0 - coverage - 0.1, 1.0, stormCloud);
                    stormCloud = pow(stormCloud, 1) * 8;
					stormCloud = saturate(stormCloud);
                    
                    return stormCloud * stormMask * intensity;
                }
                
                return 0.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // -------- FULL DAY CYCLE TIME SPLITS --------
                float midnight1 = saturate(1 - abs(_TimeOfDay - 0.0) * 8);
                float sunrise   = saturate(1 - abs(_TimeOfDay - 0.25) * 6);
                float noon      = saturate(1 - abs(_TimeOfDay - 0.5) * 6);
                float sunset    = saturate(1 - abs(_TimeOfDay - 0.75) * 6);
                float midnight2 = saturate(1 - abs(_TimeOfDay - 1.0) * 8);
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
				float sunAngle = (_TimeOfDay - 0.25) * 3.14159 * 2.0;
                float3 sunDir = normalize(float3(cos(sunAngle), sin(sunAngle), 0));
                
                // -------- RENDER SUN DISC --------
                float sunDist = distance(normalize(i.viewDir), sunDir);
                float sunDisc = 1.0 - smoothstep(_SunSize * 0.9, _SunSize, sunDist);
                // Only show sun when above horizon
				float sunVisible = step(0, sunDir.y);
                sunDisc *= sunVisible;
                // Sun glow/corona
				float sunGlow = pow(saturate(dot(i.viewDir, sunDir)), 15.0) * 0.5 * sunVisible;
                // Sun halo (larger, softer glow)
				float sunHalo = pow(saturate(dot(i.viewDir, sunDir)), 4.0) * sunVisible;
                sunHalo = sunHalo * _SunHaloIntensity * smoothstep(_SunHaloSize, 0.0, sunDist);
                
                // -------- CALCULATE MOON POSITION (ROTATION SLIDER ONLY - NO TIME) --------
                float moonAngleRad = _MoonRotationOffset * 0.0174533;
                float3 moonDir = normalize(float3(sin(moonAngleRad), cos(moonAngleRad), 0));
                
                // -------- RENDER MOON DISC --------
                float moonDist = distance(normalize(i.viewDir), moonDir);
                float moonDisc = 1.0 - smoothstep(_MoonSize * 0.9, _MoonSize, moonDist);
                // Only show moon when above horizon
				float moonVisible = step(0, moonDir.y);
                moonDisc *= moonVisible;
                // Make moon brighter and more visible
				float3 moonColor = float3(0.9, 0.9, 1.0) * 2.0;
                // Moon halo (soft atmospheric glow)
				float moonHalo = pow(saturate(dot(i.viewDir, moonDir)), 6.0) * moonVisible;
                moonHalo = moonHalo * _MoonHaloIntensity * smoothstep(_MoonHaloSize, 0.0, moonDist);

                // -------- STARS --------
				// Stars only visible during deep night: 0.0-0.13 and 0.85-1.0
                float nightIntensity = 0.0;
                if (_TimeOfDay <= 0.13)
                {
                    nightIntensity = 1.0 - (_TimeOfDay / 0.13);
                }
                else if (_TimeOfDay >= 0.85)
                {
                    nightIntensity = (_TimeOfDay - 0.85) / 0.15;
                }
                
                float3 stars = float3(0,0,0);
                if (nightIntensity > 0.01 && i.viewDir.y > 0) // Only render stars at night and above horizon
                {
                    float smallStars = generateStars(i.viewDir, _SmallStarDensity, 0.15, _SmallStarBrightness, _Time.y);
                    float largeStars = generateStars(i.viewDir, _LargeStarDensity, 0.25, _LargeStarBrightness, _Time.y * 0.7);
                    float totalStars = smallStars + largeStars;
                    stars = float3(0.9, 0.95, 1.0) * totalStars * nightIntensity;
                }

                // -------- CLOUDS --------
                float2 uv = i.viewDir.xz / max(i.viewDir.y + 1.0, 0.3);

                float lowCloud = fbm(uv*_CloudScale + _Time.y*_CloudSpeed);
                lowCloud = smoothstep((1.0 - _CloudDensity)-0.2,(1.0 - _CloudDensity)+0.2,lowCloud);
                lowCloud = pow(lowCloud,_CloudSoftness);
				// Use modulo to wrap high cloud offset smoothly without precision loss
                float2 highCloudOffset = float2(_Time.y * _HighCloudSpeed, _Time.y * _HighCloudSpeed * 0.7);
                highCloudOffset = fmod(highCloudOffset, 1000.0);
                float highCloud = fbm(uv*_HighCloudScale + highCloudOffset);
                highCloud = smoothstep((1.0 - _HighCloudDensity)-0.15,(1.0 - _HighCloudDensity)+0.15,highCloud);

                float cloudMask = saturate(lowCloud + highCloud*0.6);
                
                // -------- STORM CLOUDS --------
                float stormTotal = 0;
                
                if (_Storm1Active > 0.5)
                {
                    stormTotal += generateStorm(i.viewDir, _Storm1Position.xy, _Storm1Radius, 
                                                _Storm1Intensity, _Storm1Speed, _Storm1Coverage, _Time.y);
                }
                
                if (_Storm2Active > 0.5)
                {
                    stormTotal += generateStorm(i.viewDir, _Storm2Position.xy, _Storm2Radius, 
                                                _Storm2Intensity, _Storm2Speed, _Storm2Coverage, _Time.y);
                }
                
                if (_Storm3Active > 0.5)
                {
                    stormTotal += generateStorm(i.viewDir, _Storm3Position.xy, _Storm3Radius, 
                                                _Storm3Intensity, _Storm3Speed, _Storm3Coverage, _Time.y);
                }
                
                stormTotal = saturate(stormTotal);
                
                // Combine regular clouds with storm clouds
                float totalCloudMask = saturate(cloudMask + stormTotal);

                float shadow = lerp(1,1-cloudMask,_CloudShadowStrength);

                float3 clouds = cloudMask * sunColor * shadow;
                
                // Storm clouds are darker
                float3 stormClouds = stormTotal * _StormCloudDarkness.rgb;
                clouds = lerp(clouds, stormClouds, stormTotal);

                // -------- COMBINE EVERYTHING --------
                float3 finalColor = sky;
                
                // Apply environment darkening from storms
                float isDaytime = 0;
                if (_TimeOfDay > 0.13 && _TimeOfDay < 0.85)
                {
                    isDaytime = 1.0;
                }
                float stormDarkening = stormTotal * _EnvironmentDarkening * isDaytime;
                finalColor = lerp(finalColor, finalColor * (1.0 - stormDarkening * 0.6), stormDarkening);
                
                finalColor += stars; // Add stars behind clouds
                finalColor += clouds;

				// Add sun with halo
                finalColor += sunColor * sunHalo; // Halo first (behind disc)
                finalColor += sunColor * sunDisc * _SunIntensity;
                finalColor += sunColor * sunGlow * _SunIntensity;
                
				
			    // Add moon with halo
				finalColor += moonColor * moonHalo * 0.5;
                finalColor += moonColor * moonDisc * _MoonIntensity * 5.0;

                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
