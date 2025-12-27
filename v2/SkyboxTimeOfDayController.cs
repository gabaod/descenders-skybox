using UnityEngine;
using ModTool.Interface;

[ExecuteInEditMode]
public class SkyboxTimeOfDayController : ModBehaviour
{
    [Range(0f,1f)] public float timeOfDay = 0.5f;
    public Material skyboxMaterial;
    public Light sunLight;
    public Light moonLight;
    public Projector cloudShadowProjector;

    [Header("Time Curve")]
    public AnimationCurve timeOfDayCurve = AnimationCurve.Linear(0,0,1,1);

    [Header("Sun & Moon Settings")]
    public float sunMinAngle = -10f;
    public float sunMaxAngle = 170f;
    public float moonIntensityMax = 0.2f;

    [Header("Ambient Settings")]
    public Color nightAmbient = new Color(0.05f,0.07f,0.1f);
    public Color dayAmbient = Color.white;

    [Header("Cloud Shadow")]
    [Range(0,1)] public float cloudShadowStrength = 0.4f;

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

        // --- Update shader ---
        skyboxMaterial.SetFloat("_TimeOfDay", curveT);

        // --- Sun rotation ---
        float sunAngle = Mathf.Lerp(sunMinAngle, sunMaxAngle, curveT);
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // --- Moon rotation & intensity ---
        if (moonLight)
        {
            moonLight.transform.rotation = Quaternion.Euler(170f - sunAngle, 170f, 0f);
            moonLight.intensity = Mathf.Lerp(0f, moonIntensityMax, 1f - curveT);
        }

        // --- Ambient ---
        float ambientFactor = Mathf.Sin(curveT * Mathf.PI); // smoother day/night
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, ambientFactor);

        // --- Cloud shadows ---
        if (cloudShadowProjector)
        {
            cloudShadowProjector.transform.rotation = sunLight.transform.rotation;
            cloudShadowProjector.material.SetFloat("_ShadowStrength", cloudShadowStrength);
        }

        // --- GI refresh ---
        DynamicGI.UpdateEnvironment();
    }
}
