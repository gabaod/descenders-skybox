using UnityEngine;
using ModTool.Interface;

[ExecuteInEditMode]
public class SkyboxTimeOfDayController : ModBehaviour
{
    [Header("Full Day Cycle: 0=Midnight, 0.25=Sunrise, 0.5=Noon, 0.75=Sunset, 1=Midnight")]
    [Range(0f,1f)] public float timeOfDay = 0.5f;
    
    [Header("Materials & Lights")]
    public Material skyboxMaterial;
    public Light sunLight;
    public Light moonLight;
    public Projector cloudShadowProjector;
    
    [Header("Time Curve")]
    public AnimationCurve timeOfDayCurve = AnimationCurve.Linear(0,0,1,1);
    
    [Header("Sun & Moon Settings")]
    public float sunMinAngle = -10f;
    public float sunMaxAngle = 170f;
    public float sunIntensityMultiplier = 1.5f;
    [Range(0.01f, 0.2f)] public float sunSize = 0.05f;
    public float moonIntensityMax = 0.2f;
    [Range(0.01f, 0.2f)] public float moonSize = 0.04f;
    
    [Header("Ambient Settings")]
    public Color nightAmbient = new Color(0.05f,0.07f,0.1f);
    public Color dayAmbient = Color.white;
    
    [Header("Cloud Settings")]
    [Range(0,1)] public float cloudDensity = 0.65f;
    [Range(0.5f, 4f)] public float cloudScale = 1.6f;
    [Range(0, 0.5f)] public float cloudSpeed = 0.04f;
    [Range(0.5f, 3f)] public float cloudSoftness = 1.6f;
    
    [Header("High Cloud Settings")]
    [Range(0,1)] public float highCloudDensity = 0.35f;
    [Range(1f, 8f)] public float highCloudScale = 3.5f;
    [Range(0, 1f)] public float highCloudSpeed = 0.12f;
    
    [Header("Cloud Shadow")]
    [Range(0,1)] public float cloudShadowStrength = 0.4f;
    
    [Header("Star Settings")]
    [Range(0,1)] public float smallStarDensity = 0.5f;
    [Range(0,2)] public float smallStarBrightness = 1.0f;
    [Range(0,1)] public float largeStarDensity = 0.2f;
    [Range(0,2)] public float largeStarBrightness = 1.2f;
    [Range(0,5)] public float starTwinkleSpeed = 1.0f;
    
    void OnEnable()
    {
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;
    }
    
    void OnValidate() { UpdateSky(timeOfDay); }
    
    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateSky(timeOfDay);
#endif
        if (Application.isPlaying)
            UpdateSky(timeOfDay);
    }
    
    void UpdateSky(float t)
    {
        if (!skyboxMaterial || !sunLight) return;
        
        // --- Non-linear time ---
        float curveT = timeOfDayCurve.Evaluate(t);
        
        // --- Update shader properties ---
        skyboxMaterial.SetFloat("_TimeOfDay", curveT);
        skyboxMaterial.SetFloat("_SunSize", sunSize);
        skyboxMaterial.SetFloat("_MoonSize", moonSize);
        skyboxMaterial.SetFloat("_CloudDensity", cloudDensity);
        skyboxMaterial.SetFloat("_CloudScale", cloudScale);
        skyboxMaterial.SetFloat("_CloudSpeed", cloudSpeed);
        skyboxMaterial.SetFloat("_CloudSoftness", cloudSoftness);
        skyboxMaterial.SetFloat("_HighCloudDensity", highCloudDensity);
        skyboxMaterial.SetFloat("_HighCloudScale", highCloudScale);
        skyboxMaterial.SetFloat("_HighCloudSpeed", highCloudSpeed);
        skyboxMaterial.SetFloat("_CloudShadowStrength", cloudShadowStrength);
        skyboxMaterial.SetFloat("_SmallStarDensity", smallStarDensity);
        skyboxMaterial.SetFloat("_SmallStarBrightness", smallStarBrightness);
        skyboxMaterial.SetFloat("_LargeStarDensity", largeStarDensity);
        skyboxMaterial.SetFloat("_LargeStarBrightness", largeStarBrightness);
        skyboxMaterial.SetFloat("_StarTwinkleSpeed", starTwinkleSpeed);
        
        // --- Sun rotation (full day cycle) ---
        // Match the shader's sun calculation
        // 0.0 = midnight (below horizon), 0.25 = sunrise (horizon), 0.5 = noon (overhead), 0.75 = sunset (horizon), 1.0 = midnight
        
        // Offset by -0.25 to match shader alignment
        float sunAngleRad = (curveT - 0.25f) * Mathf.PI * 2.0f;
        
        // Convert to degrees for Unity transform
        float sunAngleDeg = sunAngleRad * Mathf.Rad2Deg;
        
        // Rotate the light to match sun position (subtract 90 to point light direction correctly)
        sunLight.transform.rotation = Quaternion.Euler(sunAngleDeg - 90f, 170f, 0f);
        
        // Calculate sun intensity based on position (visible during day, hidden at night)
        float sunVisibility = Mathf.Sin(curveT * Mathf.PI); // 0 at midnight, 1 at noon
        sunVisibility = Mathf.Clamp01(sunVisibility);
        sunLight.intensity = sunVisibility * sunIntensityMultiplier;
        
        // Update shader sun/moon intensity
        skyboxMaterial.SetFloat("_SunIntensity", sunVisibility * sunIntensityMultiplier);
        
        // --- Moon rotation & intensity ---
        if (moonLight)
        {
            // Moon should be opposite sun - when sun at 0.5 (noon), moon at 0.0 (below)
            float moonTimeOffset = curveT + 0.5f;
            if (moonTimeOffset > 1.0f) moonTimeOffset -= 1.0f;
            
            float moonAngleRad = (moonTimeOffset - 0.25f) * Mathf.PI * 2.0f;
            float moonAngleDeg = moonAngleRad * Mathf.Rad2Deg;
            moonLight.transform.rotation = Quaternion.Euler(moonAngleDeg - 90f, 170f, 0f);
            
            // Moon is bright at night (when sun is down)
            float moonVisibility = 1f - sunVisibility;
            moonLight.intensity = moonVisibility * moonIntensityMax;
            
            // Update shader moon intensity
            skyboxMaterial.SetFloat("_MoonIntensity", moonVisibility * moonIntensityMax);
        }
        
        // --- Ambient lighting (smooth transition) ---
        float ambientFactor = sunVisibility; // Use sun visibility for smooth day/night ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, ambientFactor);
        
        // --- Cloud shadows (only during day) ---
        if (cloudShadowProjector)
        {
            cloudShadowProjector.transform.rotation = sunLight.transform.rotation;
            cloudShadowProjector.material.SetFloat("_ShadowStrength", cloudShadowStrength * sunVisibility);
        }
        
        // --- GI refresh ---
        DynamicGI.UpdateEnvironment();
    }
}
