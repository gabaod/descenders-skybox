Shader "Custom/DynamicSkybox" {
    Properties {
        _SunDirection ("Sun Direction", Vector) = (0, 1, 0, 0)
        _MoonDirection ("Moon Direction", Vector) = (0, -1, 0, 0)
        _SunSize ("Sun Size", Float) = 1.0
        _MoonSize ("Moon Size", Float) = 0.8
        _SunIntensity ("Sun Intensity", Float) = 1.0
        _MoonIntensity ("Moon Intensity", Float) = 0.7
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _MoonColor ("Moon Color", Color) = (0.9, 0.9, 1, 1)
        _MoonTexture ("Moon Texture", 2D) = "white" {}
        _MoonNormal ("Moon Normal Map", 2D) = "bump" {}
        _MoonRotation ("Moon Rotation", Float) = 0.0
        _MoonLibration ("Moon Libration", Vector) = (0, 0, 0, 0)
        _MoonHaloSize ("Moon Halo Size", Float) = 1.5
        _MoonHaloIntensity ("Moon Halo Intensity", Float) = 0.3
        _MoonHaloColor ("Moon Halo Color", Color) = (0.7, 0.8, 1, 0.2)
        _SunHaloSize ("Sun Halo Size", Float) = 2.0
        _SunHaloIntensity ("Sun Halo Intensity", Float) = 0.5
        _SunHaloColor ("Sun Halo Color", Color) = (1, 0.9, 0.7, 0.3)
        _SkyColor ("Sky Color", Color) = (0.2, 0.5, 1, 1)
        _HorizonColor ("Horizon Color", Color) = (0.6, 0.8, 1, 1)
        _StarVisibility ("Star Visibility", Float) = 1.0
        _StarDensity ("Star Density", Float) = 0.5
        _StarSize ("Star Size", Float) = 2.0
        _StarBrightness ("Star Brightness", Float) = 1.0
        _StarTwinkle ("Star Twinkle", Float) = 0.3
        _StarTwinkleSpeed ("Star Twinkle Speed", Float) = 0.5
        _LargeStarPercentage ("Large Star Percentage", Float) = 0.15
        _StarTwinkleSpeed ("Star Twinkle Speed", Float) = 0.5
        _MoonPhase ("Moon Phase", Float) = 0.0
        _LowCloudCoverage ("Low Cloud Coverage", Float) = 0.3
        _LowCloudDensity ("Low Cloud Density", Float) = 0.5
        _LowCloudOffset ("Low Cloud Offset", Vector) = (0, 0, 0, 0)
        _LowCloudColor ("Low Cloud Color", Color) = (1, 1, 1, 1)
        _HighCloudCoverage ("High Cloud Coverage", Float) = 0.2
        _HighCloudDensity ("High Cloud Density", Float) = 0.3
        _HighCloudOffset ("High Cloud Offset", Vector) = (0, 0, 0, 0)
        _HighCloudColor ("High Cloud Color", Color) = (1, 1, 1, 0.8)
        _StormCloudCoverage ("Storm Cloud Coverage", Float) = 0.0
        _StormCloudDensity ("Storm Cloud Density", Float) = 0.8
        _StormCloudOffset ("Storm Cloud Offset", Vector) = (0, 0, 0, 0)
        _StormCloudColor ("Storm Cloud Color", Color) = (0.3, 0.3, 0.35, 1)
        _LightningIntensity ("Lightning Intensity", Float) = 0.0
        _LightningColor ("Lightning Color", Color) = (0.8, 0.9, 1, 1)
        _AtmosphereThickness ("Atmosphere Thickness", Float) = 0.5
        _RayleighScattering ("Rayleigh Scattering", Float) = 1.0
        _MieScattering ("Mie Scattering", Float) = 1.0
        _CloudsBelowHorizon ("Clouds Below Horizon", Float) = 0.0
        _FogDensity ("Fog Density", Float) = 0.1
        _FogColor ("Fog Color", Color) = (0.7, 0.8, 0.9, 1)
        _FogHeight ("Fog Height", Float) = 0.0
        _FogFalloff ("Fog Falloff", Float) = 1.0
    }
    
    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };
            
            float3 _SunDirection;
            float3 _MoonDirection;
            float _SunSize;
            float _MoonSize;
            float _SunIntensity;
            float _MoonIntensity;
            float4 _SunColor;
            float4 _MoonColor;
            sampler2D _MoonTexture;
            sampler2D _MoonNormal;
            float _MoonRotation;
            float2 _MoonLibration;
            float _MoonHaloSize;
            float _MoonHaloIntensity;
            float4 _MoonHaloColor;
            float _SunHaloSize;
            float _SunHaloIntensity;
            float4 _SunHaloColor;
            float4 _SkyColor;
            float4 _HorizonColor;
            float _StarVisibility;
            float _StarDensity;
            float _StarSize;
            float _StarBrightness;
            float _StarTwinkle;
            float _StarTwinkleSpeed;
            float _LargeStarPercentage;
            float _MoonPhase;
            float _LowCloudCoverage;
            float _LowCloudDensity;
            float2 _LowCloudOffset;
            float4 _LowCloudColor;
            float _HighCloudCoverage;
            float _HighCloudDensity;
            float2 _HighCloudOffset;
            float4 _HighCloudColor;
            float _StormCloudCoverage;
            float _StormCloudDensity;
            float2 _StormCloudOffset;
            float4 _StormCloudColor;
            float _LightningIntensity;
            float4 _LightningColor;
            float _AtmosphereThickness;
            float _RayleighScattering;
            float _MieScattering;
            float _CloudsBelowHorizon;
            float _FogDensity;
            float4 _FogColor;
            float _FogHeight;
            float _FogFalloff;
            
            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(v.vertex.xyz);
                return o;
            }
            
            // Improved hash function with better distribution
            float hash(float2 p) {
                p = frac(p * float2(443.897, 441.423));
                p += dot(p, p.yx + 19.19);
                return frac(p.x * p.y);
            }
            
            // Seamless 3D noise using the view direction directly
            float hash3D(float3 p) {
                p = frac(p * float3(443.897, 441.423, 437.195));
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }
            
            float noise3D(float3 p) {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(
                    lerp(
                        lerp(hash3D(i), hash3D(i + float3(1,0,0)), f.x),
                        lerp(hash3D(i + float3(0,1,0)), hash3D(i + float3(1,1,0)), f.x),
                        f.y
                    ),
                    lerp(
                        lerp(hash3D(i + float3(0,0,1)), hash3D(i + float3(1,0,1)), f.x),
                        lerp(hash3D(i + float3(0,1,1)), hash3D(i + float3(1,1,1)), f.x),
                        f.y
                    ),
                    f.z
                );
            }
            
            // Seamless FBM using 3D noise
            float fbm3D(float3 p) {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 5; i++) {
                    value += amplitude * noise3D(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }
            
            float3 drawSun(float3 viewDir, float3 sunDir) {
                float sunDist = distance(viewDir, sunDir);
                float sun = 1.0 - smoothstep(0.0, _SunSize * 0.05, sunDist);
                float halo = 1.0 - smoothstep(0.0, _SunHaloSize * 0.1, sunDist);
                halo = pow(halo, 3.0) * _SunHaloIntensity;
                return _SunColor.rgb * sun * _SunIntensity + _SunHaloColor.rgb * halo;
            }
                
            float3 drawMoon(float3 viewDir, float3 moonDir) {
                float moonDist = distance(viewDir, moonDir);
                float moonRadius = _MoonSize * 0.05;
    
                // Check if we're within the moon disc
                bool isInsideMoon = moonDist <= moonRadius;
    
                // Calculate halo from EDGE of moon disc, not center
                float distanceFromEdge = abs(moonDist - moonRadius);
                float newdistance = distanceFromEdge - 0;
                float haloSize = _MoonHaloSize * 0.1;
                //    return float4(distanceFromEdge, distanceFromEdge, distanceFromEdge, 1.0);
                //  float halo = 1.0 - smoothstep(0.0, _MoonHaloSize * 0.1, distanceFromEdge);
                float halo = 0.0;
                if (haloSize > 0.0)
                {
                    halo = 1.0 - smoothstep(0.0, haloSize, newdistance);
                    halo = pow(halo, 3.0) * _MoonHaloIntensity;
                    // If outside moon disc, just show halo
                    if (!isInsideMoon) {
                        return _MoonHaloColor.rgb * halo;
                    }
                }

                // Calculate UV coordinates for moon texture
                float3 moonUp = float3(0, 1, 0);
                float3 moonRight = normalize(cross(moonDir, moonUp));
                moonUp = normalize(cross(moonRight, moonDir));
    
                float3 toPixel = viewDir - moonDir;
                float u = dot(toPixel, moonRight) / moonRadius;
                float v = dot(toPixel, moonUp) / moonRadius;
    
                float2 moonUV = float2(u * 0.5 + 0.5, v * 0.5 + 0.5);
    
                if (_MoonRotation > 0 )
                {
                    float2 centeredUV = moonUV - 0.5;
                    float angle = _MoonRotation * 0.0174532925;
                    float cosAngle = cos(angle);
                    float sinAngle = sin(angle);

                    float2 rotatedUV;
                    rotatedUV.x = centeredUV.x * cosAngle - centeredUV.y * sinAngle;
                    rotatedUV.y = centeredUV.x * sinAngle + centeredUV.y * cosAngle;

                    moonUV = rotatedUV + 0.5;
                }
                // Apply libration (wobble effect)
                moonUV += _MoonLibration;
    
                // Circular mask for moon disc
                float discMask = 1.0 - smoothstep(0.95, 1.0, length(float2(u, v)));
    
                // Sample textures
                float4 moonTex = tex2D(_MoonTexture, moonUV);
                float3 moonNorm = UnpackNormal(tex2D(_MoonNormal, moonUV));
    
                // Calculate lighting from sun direction
                float3 moonNormal = normalize(moonNorm.x * moonRight + moonNorm.y * moonUp + moonNorm.z * moonDir);
                float3 sunDir = normalize(_SunDirection);
                float NdotL = max(0.0, dot(moonNormal, sunDir));
    
                // Moon phase shading
                float phase = _MoonPhase / 7.0;
                float phaseShade = 1.0;
                if (phase < 0.5) {
                    phaseShade = smoothstep(-1.0, 1.0, u + (1.0 - phase * 4.0));
                } else {
                    phaseShade = smoothstep(-1.0, 1.0, -u + ((phase - 0.5) * 4.0));
                }
    
                // Combine lighting
                float3 moonColor = moonTex.rgb * _MoonColor.rgb;
                moonColor *= NdotL * 0.7 + 0.3;
                moonColor *= phaseShade;
                moonColor *= _MoonIntensity;
    
                // Add halo ONLY at the edge, using additive blending
                moonColor = moonColor * discMask + _MoonHaloColor.rgb * halo;
    
                return moonColor;
            }
            float3 drawStars(float3 viewDir) {
                if (_StarVisibility < 0.01) return float3(0, 0, 0);
                
                float3 stars = float3(0, 0, 0);
                
                // Use normalized view direction for seamless wrapping
                float3 starCoord = normalize(viewDir);
                
                float densityThreshold = 1.0 - (_StarDensity * 0.5);
                
                int layers = 8;
                
                for (int i = 0; i < layers; i++) {
                    float scale = 15.0 + i * 12.0;
                    float3 starPos = starCoord * scale;
                    
                    float starNoise = hash3D(floor(starPos));
                    
                    if (starNoise > densityThreshold) {
                        float3 cellPos = frac(starPos);
                        float3 cellCenter = float3(0.5, 0.5, 0.5);
                        float starDist = length(cellPos - cellCenter);
                        
                        float brightness = hash3D(floor(starPos) + float3(i * 10, i * 20, i * 5));
                        brightness = pow(brightness, 0.5);
                        
                        float sizeVariation = hash3D(floor(starPos) + float3(i * 5, i * 15, i * 25));
                        float starScale;
                        
                        if (sizeVariation < _LargeStarPercentage) {
                            starScale = _StarSize * lerp(1.5, 2.5, sizeVariation / _LargeStarPercentage);
                        } else {
                            float normalizedSize = (sizeVariation - _LargeStarPercentage) / (1.0 - _LargeStarPercentage);
                            starScale = _StarSize * lerp(0.4, 1.2, normalizedSize);
                        }
                        
                        float twinklePhase = _Time.y * _StarTwinkleSpeed * 8.0 + brightness * 50.0 + i * 3.14;
                        float twinkle = 1.0 + sin(twinklePhase) * _StarTwinkle * brightness;
                        
                        float starRadius = starScale * 0.008;
                        float star = 1.0 - smoothstep(0.0, starRadius, starDist);
                        star = pow(star, 1.5);
                        
                        stars += star * brightness * twinkle * 1.2;
                    }
                }
                
                // Tiny stars layer
                for (int j = 0; j < 3; j++) {
                    float3 tinyStarPos = starCoord * (80.0 + j * 40.0);
                    float tinyNoise = hash3D(floor(tinyStarPos));
                    
                    if (tinyNoise > (densityThreshold + 0.1)) {
                        float3 tinyCell = frac(tinyStarPos);
                        float tinyDist = length(tinyCell - 0.5);
                        float tinyBrightness = hash3D(floor(tinyStarPos) + float3(j, j, j)) * 0.6;
                        float tinyStar = 1.0 - smoothstep(0.0, 0.003, tinyDist);
                        stars += tinyStar * tinyBrightness * 0.8;
                    }
                }
                
                return stars * _StarVisibility * _StarBrightness * 2.5;
            }
            
            // Seamless cloud function using 3D coordinates
            float drawClouds(float3 viewDir, float2 offset, float coverage, float density) {
                // Use 3D position with offset applied in a seamless way
                float3 cloudPos = viewDir * 3.0;
                cloudPos.xz += offset;
                
                float clouds = fbm3D(cloudPos);
                clouds = smoothstep(1.0 - coverage, 1.0, clouds);
                clouds *= density;
                
                // Optional horizon fade based on _CloudsBelowHorizon parameter
                // When 0: clouds fade at horizon, When 1: clouds wrap fully around sphere
                float horizonFade = lerp(
                    smoothstep(-0.15, 0.05, viewDir.y),  // Fade at horizon
                    1.0,                                   // No fade (full sphere)
                    _CloudsBelowHorizon
                );
                clouds *= horizonFade;
                
                return clouds;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                float3 viewDir = normalize(i.viewDir);
                
                // Sky gradient
                float horizonBlend = pow(1.0 - abs(viewDir.y), 2.0);
                float3 skyColor = lerp(_SkyColor.rgb, _HorizonColor.rgb, horizonBlend);
                
                // Atmospheric scattering
                float sunDot = dot(viewDir, normalize(_SunDirection));
                float rayleigh = pow(1.0 - sunDot * 0.5, 2.0) * _RayleighScattering;
                float mie = pow(max(0.0, sunDot), 10.0) * _MieScattering;
                skyColor += (rayleigh + mie) * _AtmosphereThickness * 0.3;
                
                // Atmospheric fog/haze - only visible near horizon
                // This creates a realistic distance haze that doesn't affect the whole scene
                if (_FogDensity > 0.01) {
                    // Calculate how close to horizon we are (0 = zenith/nadir, 1 = horizon)
                    float horizonDistance = 1.0 - abs(viewDir.y);
                    
                    // Height-based fog - more fog at lower angles
                    float heightFactor = 1.0;
                    if (viewDir.y < _FogHeight) {
                        // Below fog height - increase fog density
                        float heightDiff = _FogHeight - viewDir.y;
                        heightFactor = 1.0 + heightDiff * _FogFalloff * 2.0;
                    }
                    
                    // Fog intensity increases near horizon with smooth falloff
                    float fogAmount = pow(horizonDistance, 2.0 - _FogDensity) * _FogDensity * heightFactor;
                    fogAmount = saturate(fogAmount);
                    
                    // Blend fog color with sky color
                    skyColor = lerp(skyColor, _FogColor.rgb, fogAmount * 0.6);
                }
                
                // Draw celestial bodies
                float3 sun = drawSun(viewDir, normalize(_SunDirection));
                float3 moon = drawMoon(viewDir, normalize(_MoonDirection));
                float3 stars = drawStars(viewDir);
                
                // Cloud layers - now using 3D seamless noise
                float lowClouds = drawClouds(viewDir, _LowCloudOffset, _LowCloudCoverage, _LowCloudDensity);
                float highClouds = drawClouds(viewDir * 0.5, _HighCloudOffset, _HighCloudCoverage, _HighCloudDensity);
                float stormClouds = drawClouds(viewDir * 1.5, _StormCloudOffset, _StormCloudCoverage, _StormCloudDensity);
                
                // Combine everything
                float3 finalColor = skyColor + sun + moon + stars;
                
                // Apply clouds
                finalColor = lerp(finalColor, _LowCloudColor.rgb, lowClouds * _LowCloudColor.a);
                finalColor = lerp(finalColor, _HighCloudColor.rgb, highClouds * _HighCloudColor.a);
                finalColor = lerp(finalColor, _StormCloudColor.rgb, stormClouds);
                
                // Lightning effect - using 3D hash for seamless lightning
                if (_LightningIntensity > 0.01 && stormClouds > 0.6) {
                    float lightningFlash = _LightningIntensity * stormClouds * hash3D(viewDir * 100.0 + _Time.y);
                    finalColor += _LightningColor.rgb * lightningFlash * 0.3;
                }
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
