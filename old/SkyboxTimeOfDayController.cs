using UnityEngine;
using ModTool.Interface;

[ExecuteInEditMode]
public class SkyboxTimeOfDayController : ModBehaviour
{
    [Header("Full Day Cycle: 0=Midnight, 0.25=Sunrise, 0.5=Noon, 0.75=Sunset, 1=Midnight")]
    [Range(0f, 1f)] public float timeOfDay = 0.5f;

    [Header("Materials & Lights")]
    public Material skyboxMaterial;
    public Light sunLight;
    public Light moonLight;
    public Projector cloudShadowProjector;

    [Header("Time Curve")]
    public AnimationCurve timeOfDayCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Sun & Moon Settings")]
    public float sunMinAngle = -10f;
    public float sunMaxAngle = 170f;
    public float sunIntensityMultiplier = 1.5f;
    [Range(0.01f, 0.2f)] public float sunSize = 0.05f;
    [Range(0.1f, 1.0f)] public float sunHaloSize = 0.3f;
    [Range(0f, 2f)] public float sunHaloIntensity = 0.5f;
    public float moonIntensityMax = 0.2f;
    [Range(0.01f, 0.2f)] public float moonSize = 0.04f;
    [Range(0.1f, 1.0f)] public float moonHaloSize = 0.25f;
    [Range(0f, 2f)] public float moonHaloIntensity = 0.3f;
    [Range(0f, 360f)] public float moonRotationOffset = 180f;

    [Header("Ambient Settings")]
    public Color nightAmbient = new Color(0.05f, 0.07f, 0.1f);
    public Color dayAmbient = Color.white;
    public Color moonPeakAmbient = new Color(0.15f, 0.18f, 0.25f);
    [Range(0f, 2f)] public float moonPeakAmbientIntensity = 1.0f;

    [Header("Cloud Settings")]
    [Range(0, 1)] public float cloudDensity = 0.65f;
    [Range(0.5f, 4f)] public float cloudScale = 1.6f;
    [Range(0, 0.5f)] public float cloudSpeed = 0.04f;
    [Range(0.5f, 3f)] public float cloudSoftness = 1.6f;

    [Header("High Cloud Settings")]
    [Range(0, 1)] public float highCloudDensity = 0.35f;
    [Range(1f, 8f)] public float highCloudScale = 3.5f;
    [Range(0, 1f)] public float highCloudSpeed = 0.12f;

    [Header("Cloud Shadow")]
    [Range(0, 1)] public float cloudShadowStrength = 0.4f;

    [Header("Star Settings")]
    [Range(0, 1)] public float smallStarDensity = 0.5f;
    [Range(0, 2)] public float smallStarBrightness = 1.0f;
    [Range(0, 1)] public float largeStarDensity = 0.2f;
    [Range(0, 2)] public float largeStarBrightness = 1.2f;
    [Range(0, 5)] public float starTwinkleSpeed = 1.0f;

    [Header("Storm System")]
    [Range(0, 1)] public float environmentDarkening = 0.5f;
    public Color stormCloudDarkness = new Color(0.2f, 0.2f, 0.25f);

    [Header("Storm 1")]
    public bool storm1Active = false;
    public Vector2 storm1Position = new Vector2(0, 0);
    [Range(10f, 200f)] public float storm1Radius = 50f;
    [Range(0f, 1f)] public float storm1Intensity = 0.5f;
    [Range(0f, 0.5f)] public float storm1Speed = 0.1f;
    [Range(0f, 1f)] public float storm1Coverage = 0.7f;

    [Header("Storm 2")]
    public bool storm2Active = false;
    public Vector2 storm2Position = new Vector2(50, 50);
    [Range(10f, 200f)] public float storm2Radius = 50f;
    [Range(0f, 1f)] public float storm2Intensity = 0.5f;
    [Range(0f, 0.5f)] public float storm2Speed = 0.1f;
    [Range(0f, 1f)] public float storm2Coverage = 0.7f;

    [Header("Storm 3")]
    public bool storm3Active = false;
    public Vector2 storm3Position = new Vector2(-50, -50);
    [Range(10f, 200f)] public float storm3Radius = 50f;
    [Range(0f, 1f)] public float storm3Intensity = 0.5f;
    [Range(0f, 0.5f)] public float storm3Speed = 0.1f;
    [Range(0f, 1f)] public float storm3Coverage = 0.7f;

    void OnEnable()
    {
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;
    }

    void OnValidate()
    {
        if (skyboxMaterial != null)
        {
            // Force update moon rotation immediately
            skyboxMaterial.SetFloat("_MoonRotationOffset", moonRotationOffset);
        }
        UpdateSky(timeOfDay);
    }

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
        skyboxMaterial.SetFloat("_MoonRotationOffset", moonRotationOffset); // Pass as degrees
        skyboxMaterial.SetFloat("_SunHaloSize", sunHaloSize);
        skyboxMaterial.SetFloat("_SunHaloIntensity", sunHaloIntensity);
        skyboxMaterial.SetFloat("_MoonHaloSize", moonHaloSize);
        skyboxMaterial.SetFloat("_MoonHaloIntensity", moonHaloIntensity);
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

        // Storm system
        skyboxMaterial.SetFloat("_EnvironmentDarkening", environmentDarkening);
        skyboxMaterial.SetColor("_StormCloudDarkness", stormCloudDarkness);

        skyboxMaterial.SetFloat("_Storm1Active", storm1Active ? 1f : 0f);
        skyboxMaterial.SetVector("_Storm1Position", storm1Position);
        skyboxMaterial.SetFloat("_Storm1Radius", storm1Radius);
        skyboxMaterial.SetFloat("_Storm1Intensity", storm1Intensity);
        skyboxMaterial.SetFloat("_Storm1Speed", storm1Speed);
        skyboxMaterial.SetFloat("_Storm1Coverage", storm1Coverage);

        skyboxMaterial.SetFloat("_Storm2Active", storm2Active ? 1f : 0f);
        skyboxMaterial.SetVector("_Storm2Position", storm2Position);
        skyboxMaterial.SetFloat("_Storm2Radius", storm2Radius);
        skyboxMaterial.SetFloat("_Storm2Intensity", storm2Intensity);
        skyboxMaterial.SetFloat("_Storm2Speed", storm2Speed);
        skyboxMaterial.SetFloat("_Storm2Coverage", storm2Coverage);

        skyboxMaterial.SetFloat("_Storm3Active", storm3Active ? 1f : 0f);
        skyboxMaterial.SetVector("_Storm3Position", storm3Position);
        skyboxMaterial.SetFloat("_Storm3Radius", storm3Radius);
        skyboxMaterial.SetFloat("_Storm3Intensity", storm3Intensity);
        skyboxMaterial.SetFloat("_Storm3Speed", storm3Speed);
        skyboxMaterial.SetFloat("_Storm3Coverage", storm3Coverage);

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

            // Apply user-defined rotation offset
            moonAngleRad += moonRotationOffset * Mathf.Deg2Rad;

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

        // Calculate moon peak ambient boost (0.0-0.06 and 0.93-1.0)
        float moonPeakFactor = 0f;
        if (curveT <= 0.06f)
        {
            // Fade in from 0.0 to 0.06
            moonPeakFactor = (0.06f - curveT) / 0.06f;
        }
        else if (curveT >= 0.93f)
        {
            // Fade in from 0.93 to 1.0
            moonPeakFactor = (curveT - 0.93f) / 0.07f;
        }

        // Blend between night ambient and moon peak ambient
        Color currentNightAmbient = Color.Lerp(nightAmbient, moonPeakAmbient, moonPeakFactor * moonPeakAmbientIntensity);

        // Final ambient: blend between day and enhanced night ambient
        Color baseAmbient = Color.Lerp(currentNightAmbient, dayAmbient, ambientFactor);

        // Calculate storm darkening (only during daytime: 0.13-0.85)
        float stormDarkeningFactor = 0f;
        if (curveT > 0.13f && curveT < 0.85f)
        {
            float totalStormIntensity = 0f;
            if (storm1Active) totalStormIntensity += storm1Intensity;
            if (storm2Active) totalStormIntensity += storm2Intensity;
            if (storm3Active) totalStormIntensity += storm3Intensity;

            // Average the storm intensities and apply darkening
            totalStormIntensity = Mathf.Clamp01(totalStormIntensity / 3f);
            stormDarkeningFactor = totalStormIntensity * environmentDarkening;
        }

        // Apply storm darkening to ambient light
        RenderSettings.ambientLight = Color.Lerp(baseAmbient, baseAmbient * (1f - stormDarkeningFactor * 0.7f), stormDarkeningFactor);

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
