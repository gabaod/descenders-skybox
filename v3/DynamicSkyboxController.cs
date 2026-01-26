using UnityEngine;
using ModTool.Interface;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class DynamicSkyboxController : ModBehaviour
{
	[Header("Time of Day")]
	[Range(0f, 24f)]
	public float timeOfDay = 12f;
	[Range(0.1f, 10f)]
	public float timeSpeed = 1f;
	public bool autoAdvanceTime = false;

	[Header("Sun Settings")]
	[Range(0f, 23.99f)]
	public float sunriseTime = 6f;
	[Range(0f, 23.99f)]
	public float sunsetTime = 18f;
	[Range(0f, 360f)]
	public float sunPathRotation = 0f;
	[Range(0.1f, 5f)]
	public float sunSize = 1f;
	[Range(0f, 2f)]
	public float sunIntensity = 1f;
	[Range(0f, 5f)]
	public float sunHaloSize = 2f;
	[Range(0f, 1f)]
	public float sunHaloIntensity = 0.5f;
	public Color sunColor = new Color(1f, 0.95f, 0.8f);
	public Color sunHaloColor = new Color(1f, 0.9f, 0.7f, 0.3f);

	[Header("Moon Settings")]
	[Range(0f, 360f)]
	public float moonRotationOffset = 180f;
	[Range(0f, 360f)]
	public float moonOrbitRotation = 0f;
	[Range(0.1f, 3f)]
	public float moonSize = 0.8f;
	[Range(0f, 2f)]
	public float moonIntensity = 0.7f;
	[Range(0f, 5f)]
	public float moonHaloSize = 1.5f;
	[Range(0f, 1f)]
	public float moonHaloIntensity = 0.3f;
	public Color moonHaloColor = new Color(0.7f, 0.8f, 1f, 0.2f);
	[Range(0f, 7f)]
	public float moonPhase = 0f;
	public Color moonColor = new Color(0.9f, 0.9f, 1f);

	[Header("Moon Textures")]
	public Texture2D moonAlbedoTexture;
	public Texture2D moonNormalTexture;
	[Range(0f, 360f)]
	public float moonRotationSpeed = 1f;
	[Range(0f, 5f)]
	public float moonLibrationAmount = 1f;
	[Range(0f, 10f)]
	public float moonLibrationSpeed = 1f;

	[Header("Sky Colors")]
	public Gradient skyGradient;
	public Gradient horizonGradient;
	public Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f);
	public Color dayZenithColor = new Color(0.2f, 0.5f, 1f);
	public Color sunriseColor = new Color(1f, 0.6f, 0.4f);
	public Color sunsetColor = new Color(1f, 0.4f, 0.3f);

	[Header("Stars")]
	[Range(0f, 1f)]
	public float starVisibility = 1f;
	[Range(0f, 1f)]
	public float starDensity = 0.5f;
	[Range(0f, 1f)]
	public float starBrightness = 0.75f;
	[Range(0.5f, 3f)]
	public float starSize = 1f;
	[Range(0f, 1f)]
	public float starTwinkleSpeed = 0.5f;
	[Range(0f, 1f)]
	public float starTwinkleAmount = 0.3f;
	[Range(0f, 1f)]
	public float largeStarPercentage = 0.1f;

	[Header("Cloud Layers")]
	[Range(0f, 1f)]
	public float lowCloudCoverage = 0.3f;
	[Range(0f, 1f)]
	public float lowCloudDensity = 0.5f;
	[Range(0.1f, 5f)]
	public float lowCloudSpeed = 1f;
	public Color lowCloudColor = Color.white;

	[Range(0f, 1f)]
	public float highCloudCoverage = 0.2f;
	[Range(0f, 1f)]
	public float highCloudDensity = 0.3f;
	[Range(0.1f, 5f)]
	public float highCloudSpeed = 0.5f;
	public Color highCloudColor = new Color(1f, 1f, 1f, 0.8f);

	[Range(0f, 1f)]
	public float stormCloudCoverage = 0f;
	[Range(0f, 1f)]
	public float stormCloudDensity = 0.8f;
	[Range(0.1f, 3f)]
	public float stormCloudSpeed = 2f;
	public Color stormCloudColor = new Color(0.3f, 0.3f, 0.35f);

	[Header("Lightning")]
	[Range(0f, 1f)]
	public float lightningFrequency = 0f;
	[Range(0.1f, 2f)]
	public float lightningDuration = 0.2f;
	[Range(0f, 10f)]
	public float lightningIntensity = 5f;
	public Color lightningColor = new Color(0.8f, 0.9f, 1f);

	[Header("Atmosphere")]
	[Range(0f, 1f)]
	public float atmosphereThickness = 0.5f;
	[Range(0f, 2f)]
	public float rayleighScattering = 1f;
	[Range(0f, 2f)]
	public float mieScattering = 1f;
	[Range(0f, 1f)]
	public float cloudsBelowHorizon = 0f;

	[Header("Atmospheric Fog")]
	[Range(0f, 1f)]
	public float fogDensity = 0.1f;
	public Color fogColor = new Color(0.7f, 0.8f, 0.9f);
	[Range(-1f, 1f)]
	public float fogHeight = 0f;
	[Range(0f, 5f)]
	public float fogFalloff = 1f;

	[Header("Lighting")]
	[Range(0f, 2f)]
	public float dayAmbientIntensity = 1f;
	[Range(0f, 1f)]
	public float nightAmbientIntensity = 0.2f;
	[Range(0f, 2f)]
	public float terrainLightMultiplier = 1f;
	public Light directionalLight;

	[Header("Material Setup")]
	public Material existingSkyboxMaterial;
	public bool createNewMaterial = true;

	[Header("Weather Presets")]
	public bool useClearSky = false;
	public bool usePartlyCloudy = false;
	public bool useOvercast = false;
	public bool useStormy = false;

	// Private variables
	private Material skyboxMaterial;
	private float lightningTimer = 0f;
	private bool lightningActive = false;
	private float currentLightningIntensity = 0f;
	private Vector2 lowCloudOffset;
	private Vector2 highCloudOffset;
	private Vector2 stormCloudOffset;
	private float moonRotation = 0f;

	private bool isInitialized = false;

	void OnEnable()
	{
		Debug.Log("DynamicSkyboxController: OnEnable called");
		Debug.Log("Initial values - TimeOfDay: " + timeOfDay + ", AutoAdvanceTime: " + autoAdvanceTime + ", TimeSpeed: " + timeSpeed);
		Debug.Log("Sunrise: " + sunriseTime + ", Sunset: " + sunsetTime);
		if (!isInitialized)
		{
			SetupSkybox();
			SetupGradients();
			isInitialized = true;
			Debug.Log("DynamicSkyboxController: Initialized successfully");
		}

#if UNITY_EDITOR
		// Force Unity Editor to continuously update this script in Edit Mode
		EditorApplication.update += EditorUpdate;
#endif
	}

	void OnDisable()
	{
#if UNITY_EDITOR
		EditorApplication.update -= EditorUpdate;
#endif
	}

#if UNITY_EDITOR
	void EditorUpdate()
	{
		if (!Application.isPlaying && autoAdvanceTime)
		{
			// Force Update() to be called every editor frame when auto-advance is enabled
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}
	}
#endif

	void SetupSkybox()
	{
		Debug.Log("DynamicSkyboxController: SetupSkybox called");

		// Use existing material if provided, otherwise create new one
		if (existingSkyboxMaterial != null)
		{
			skyboxMaterial = existingSkyboxMaterial;
			Debug.Log("Using existing skybox material: " + existingSkyboxMaterial.name);
		}
		else if (createNewMaterial)
		{
			// Create custom skybox material
			Shader skyShader = Shader.Find("Custom/DynamicSkybox");
			if (skyShader != null)
			{
				skyboxMaterial = new Material(skyShader);
				skyboxMaterial.name = "Dynamic Skybox (Runtime)";
				Debug.Log("Created new skybox material at runtime");
			}
			else
			{
				Debug.LogError("DynamicSkybox shader not found! Please create the shader file 'Custom/DynamicSkybox' in your project.");
				return;
			}
		}
		else
		{
			Debug.LogWarning("No skybox material assigned and createNewMaterial is false. Please assign a material or enable createNewMaterial.");
			return;
		}

		RenderSettings.skybox = skyboxMaterial;
		DynamicGI.UpdateEnvironment();
		Debug.Log("Skybox set to RenderSettings. AutoAdvanceTime: " + autoAdvanceTime + ", TimeSpeed: " + timeSpeed);


		// Find existing directional light - DO NOT CREATE
		if (directionalLight == null)
		{
			Light[] lights = FindObjectsOfType<Light>();
			foreach (Light light in lights)
			{
				if (light.type == LightType.Directional)
				{
					directionalLight = light;
					Debug.Log("Found directional light: " + light.name);
					break;
				}
			}

			if (directionalLight == null)
			{
				Debug.LogWarning("No directional light found in scene. Please assign one manually or add a Directional Light to your scene.");
			}
		}
	}

	void SetupGradients()
	{
		if (skyGradient == null)
		{
			skyGradient = new Gradient();
			GradientColorKey[] colorKeys = new GradientColorKey[5];
			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];

			colorKeys[0] = new GradientColorKey(nightSkyColor, 0f);
			colorKeys[1] = new GradientColorKey(sunriseColor, 0.25f);
			colorKeys[2] = new GradientColorKey(dayZenithColor, 0.5f);
			colorKeys[3] = new GradientColorKey(sunsetColor, 0.75f);
			colorKeys[4] = new GradientColorKey(nightSkyColor, 1f);

			for (int i = 0; i < 5; i++)
				alphaKeys[i] = new GradientAlphaKey(1f, i * 0.25f);

			skyGradient.SetKeys(colorKeys, alphaKeys);
		}

		if (horizonGradient == null)
		{
			horizonGradient = new Gradient();
			GradientColorKey[] hColorKeys = new GradientColorKey[5];
			GradientAlphaKey[] hAlphaKeys = new GradientAlphaKey[5];

			hColorKeys[0] = new GradientColorKey(nightSkyColor, 0f);
			hColorKeys[1] = new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0.25f);
			hColorKeys[2] = new GradientColorKey(new Color(0.6f, 0.8f, 1f), 0.5f);
			hColorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.75f);
			hColorKeys[4] = new GradientColorKey(nightSkyColor, 1f);

			for (int i = 0; i < 5; i++)
				hAlphaKeys[i] = new GradientAlphaKey(1f, i * 0.25f);

			horizonGradient.SetKeys(hColorKeys, hAlphaKeys);
		}
	}

	void OnValidate()
	{
#if UNITY_EDITOR
		if (skyboxMaterial == null && (existingSkyboxMaterial != null || createNewMaterial))
		{
			SetupSkybox();
			SetupGradients();
		}
		UpdateSkybox();
		UpdateLighting();
#endif
	}

	void Update()
	{
		try
		{
			// Ensure initialization happens even if OnEnable was missed
			if (!isInitialized)
			{
				Debug.Log("DynamicSkyboxController: Late initialization in Update");
				SetupSkybox();
				SetupGradients();
				isInitialized = true;
			}

			// Debug Time.deltaTime to see if it's zero
			if (Time.frameCount % 60 == 0)
			{
				Debug.Log("Time.deltaTime: " + Time.deltaTime + ", Application.isPlaying: " + Application.isPlaying);
			}

			// Auto advance time
			if (autoAdvanceTime)
			{
				float previousTime = timeOfDay;
				float deltaTime = Time.deltaTime * timeSpeed / 3600f * 24f;

				Debug.Log("BEFORE increment - timeOfDay: " + timeOfDay.ToString("F4") +
						  ", deltaTime: " + deltaTime.ToString("F6") +
						  ", sunsetTime: " + sunsetTime.ToString("F4"));

				timeOfDay += deltaTime;

				Debug.Log("AFTER increment - timeOfDay: " + timeOfDay.ToString("F4"));

				// Debug time progression
				if (Time.frameCount % 60 == 0) // Every second at 60fps
				{
					Debug.Log("Time progression - Previous: " + previousTime.ToString("F2") +
							  ", Delta: " + deltaTime.ToString("F4") +
							  ", New: " + timeOfDay.ToString("F2") +
							  ", Sunset: " + sunsetTime.ToString("F2"));
				}

				if (timeOfDay >= 24f)
				{
					Debug.Log("Time wrapped from " + timeOfDay.ToString("F2") + " to " + (timeOfDay - 24f).ToString("F2"));
					timeOfDay -= 24f;
				}

				// Debug every hour change
				if (Mathf.FloorToInt(previousTime) != Mathf.FloorToInt(timeOfDay))
				{
					Debug.Log("DynamicSkyboxController: Time advanced to " + timeOfDay.ToString("F2") + " hours");
				}
			}
			else
			{
				// Debug once to show auto-advance is off
				if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
				{
					Debug.Log("DynamicSkyboxController: AutoAdvanceTime is OFF. Current time: " + timeOfDay.ToString("F2"));
				}
			}

			// Apply weather presets
			ApplyWeatherPresets();

			// Update cloud offsets
			lowCloudOffset += new Vector2(lowCloudSpeed, lowCloudSpeed * 0.5f) * Time.deltaTime * 0.01f;
			highCloudOffset += new Vector2(highCloudSpeed, highCloudSpeed * 0.3f) * Time.deltaTime * 0.01f;
			stormCloudOffset += new Vector2(stormCloudSpeed, stormCloudSpeed * 0.7f) * Time.deltaTime * 0.01f;

			// Update lightning
			UpdateLightning();

			// Update skybox
			UpdateSkybox();

			// Update lighting
			UpdateLighting();

			Debug.Log("Update completed successfully");
		}
		catch (System.Exception e)
		{
			Debug.LogError("DynamicSkyboxController Update() exception: " + e.Message + "\n" + e.StackTrace);
		}
	}

	void ApplyWeatherPresets()
	{
		if (useClearSky)
		{
			lowCloudCoverage = 0f;
			highCloudCoverage = 0.1f;
			stormCloudCoverage = 0f;
			lightningFrequency = 0f;
			useClearSky = false;
		}
		else if (usePartlyCloudy)
		{
			lowCloudCoverage = 0.4f;
			highCloudCoverage = 0.3f;
			stormCloudCoverage = 0f;
			lightningFrequency = 0f;
			usePartlyCloudy = false;
		}
		else if (useOvercast)
		{
			lowCloudCoverage = 0.8f;
			highCloudCoverage = 0.6f;
			stormCloudCoverage = 0.2f;
			lightningFrequency = 0f;
			useOvercast = false;
		}
		else if (useStormy)
		{
			lowCloudCoverage = 0.6f;
			highCloudCoverage = 0.4f;
			stormCloudCoverage = 0.9f;
			lightningFrequency = 0.3f;
			useStormy = false;
		}
	}

	void UpdateLightning()
	{
		if (lightningFrequency > 0f && stormCloudCoverage > 0.3f)
		{
			lightningTimer += Time.deltaTime;

			if (!lightningActive && Random.value < lightningFrequency * Time.deltaTime)
			{
				lightningActive = true;
				lightningTimer = 0f;
				currentLightningIntensity = lightningIntensity;
			}

			if (lightningActive)
			{
				if (lightningTimer > lightningDuration)
				{
					lightningActive = false;
					currentLightningIntensity = 0f;
				}
				else
				{
					float flickerPattern = Random.value > 0.7f ? Random.value : 1f;
					currentLightningIntensity = lightningIntensity * flickerPattern *
						(1f - lightningTimer / lightningDuration);
				}
			}
		}
		else
		{
			lightningActive = false;
			currentLightningIntensity = 0f;
		}
	}

	void UpdateSkybox()
	{
		if (skyboxMaterial == null) return;

		// Validate sunrise/sunset times
		if (sunsetTime <= sunriseTime)
			sunsetTime = sunriseTime + 0.1f;

		Debug.Log("UpdateSkybox called - TimeOfDay: " + timeOfDay.ToString("F2") + ", Sunrise: " + sunriseTime + ", Sunset: " + sunsetTime);

		// Calculate sun position based on sunrise and sunset times
		float sunDayDuration = sunsetTime - sunriseTime;
		float sunNightDuration = 24f - sunDayDuration;

		Vector3 sunDirection = Vector3.down;
		float sunVisibility = 0f;
		float sunProgress = 0f;
		bool isSunUp = false;

		if (timeOfDay >= sunriseTime && timeOfDay <= sunsetTime)
		{
			sunProgress = (timeOfDay - sunriseTime) / sunDayDuration;
			float sunAngle = sunProgress * 180f;
			sunDirection = Quaternion.Euler(-sunAngle, sunPathRotation, 0) * Vector3.forward;
			sunVisibility = 1f;
			isSunUp = true;

			if (sunProgress < 0.05f)
				sunVisibility = sunProgress / 0.05f;
			else if (sunProgress > 0.95f)
				sunVisibility = (1f - sunProgress) / 0.05f;
		}
		else
		{
			sunVisibility = 0f;
			isSunUp = false;

			float nightProgress;
			if (timeOfDay > sunsetTime)
				nightProgress = (timeOfDay - sunsetTime) / sunNightDuration;
			else
				nightProgress = (timeOfDay + (24f - sunsetTime)) / sunNightDuration;

			float underHorizonAngle = 180f + (nightProgress * 180f);
			sunDirection = Quaternion.Euler(-underHorizonAngle, sunPathRotation, 0) * Vector3.forward;
		}

		float normalizedTime = 0.5f;

		if (isSunUp)
		{
			if (sunProgress < 0.15f)
				normalizedTime = Mathf.Lerp(0.25f, 0.5f, sunProgress / 0.15f);
			else if (sunProgress > 0.85f)
				normalizedTime = Mathf.Lerp(0.5f, 0.75f, (sunProgress - 0.85f) / 0.15f);
			else
				normalizedTime = 0.5f;
		}
		else
		{
			float nightProgress;
			if (timeOfDay > sunsetTime)
				nightProgress = (timeOfDay - sunsetTime) / sunNightDuration;
			else
				nightProgress = (timeOfDay + (24f - sunsetTime)) / sunNightDuration;

			if (nightProgress < 0.15f)
				normalizedTime = Mathf.Lerp(0.75f, 1.0f, nightProgress / 0.15f);
			else if (nightProgress > 0.85f)
				normalizedTime = Mathf.Lerp(0.0f, 0.25f, (nightProgress - 0.85f) / 0.15f);
			else
				normalizedTime = 0.0f;
		}

		float moonTimeOffset = moonRotationOffset / 15f;
		float adjustedMoonTime = timeOfDay - moonTimeOffset;
		if (adjustedMoonTime < 0) adjustedMoonTime += 24f;
		if (adjustedMoonTime >= 24f) adjustedMoonTime -= 24f;

		float moonAngle = (adjustedMoonTime / 24f * 360f) - 90f;
		Vector3 moonDirection = Quaternion.Euler(moonAngle, moonOrbitRotation, 0) * Vector3.forward;

		Color currentSkyColor = skyGradient.Evaluate(normalizedTime);
		Color currentHorizonColor = horizonGradient.Evaluate(normalizedTime);

		float calculatedStarVisibility = 0f;
		if (timeOfDay < sunriseTime || timeOfDay > sunsetTime)
		{
			float distanceFromSun;
			if (timeOfDay < sunriseTime)
				distanceFromSun = Mathf.Min(sunriseTime - timeOfDay, timeOfDay + (24f - sunsetTime));
			else
				distanceFromSun = Mathf.Min(timeOfDay - sunsetTime, (24f - timeOfDay) + sunriseTime);

			calculatedStarVisibility = Mathf.Clamp01(distanceFromSun / 1f);
		}
		calculatedStarVisibility *= starVisibility;

		float adjustedSunIntensity = sunIntensity * sunVisibility;
		moonRotation += moonRotationSpeed * Time.deltaTime;
		if (moonRotation >= 360f) moonRotation -= 360f;
		skyboxMaterial.SetFloat("_MoonRotation", moonRotation);
		Debug.Log("Moon Rotation: " + moonRotation);

		skyboxMaterial.SetVector("_SunDirection", sunDirection);
		skyboxMaterial.SetVector("_MoonDirection", moonDirection);
		skyboxMaterial.SetFloat("_SunSize", sunSize);
		skyboxMaterial.SetFloat("_MoonSize", moonSize);
		skyboxMaterial.SetFloat("_SunIntensity", adjustedSunIntensity);
		skyboxMaterial.SetFloat("_MoonIntensity", moonIntensity);
		skyboxMaterial.SetColor("_SunColor", sunColor);
		skyboxMaterial.SetColor("_MoonColor", moonColor);
		skyboxMaterial.SetFloat("_SunHaloSize", sunHaloSize);
		skyboxMaterial.SetFloat("_SunHaloIntensity", sunHaloIntensity * sunVisibility);
		skyboxMaterial.SetColor("_SunHaloColor", sunHaloColor);

		skyboxMaterial.SetFloat("_MoonHaloSize", moonHaloSize);
		skyboxMaterial.SetFloat("_MoonHaloIntensity", moonHaloIntensity);
		skyboxMaterial.SetColor("_MoonHaloColor", moonHaloColor);
		//skyboxMaterial.SetColor("_MoonColor", Color.red);
		// Moon textures and animation
		if (moonAlbedoTexture != null)
			skyboxMaterial.SetTexture("_MoonTexture", moonAlbedoTexture);
		else
			skyboxMaterial.SetTexture("_MoonTexture", Texture2D.whiteTexture);

		if (moonNormalTexture != null)
			skyboxMaterial.SetTexture("_MoonNormal", moonNormalTexture);
		else
			skyboxMaterial.SetTexture("_MoonNormal", null);

		// Calculate libration (wobble effect)
		float librationX = Mathf.Sin(Time.time * moonLibrationSpeed) * moonLibrationAmount * 0.01f;
		float librationY = Mathf.Cos(Time.time * moonLibrationSpeed * 0.7f) * moonLibrationAmount * 0.01f;
		skyboxMaterial.SetVector("_MoonLibration", new Vector2(librationX, librationY));

		skyboxMaterial.SetColor("_SkyColor", currentSkyColor);
		skyboxMaterial.SetColor("_HorizonColor", currentHorizonColor);

		skyboxMaterial.SetFloat("_StarVisibility", calculatedStarVisibility);
		skyboxMaterial.SetFloat("_StarDensity", starDensity);
		skyboxMaterial.SetFloat("_StarSize", starSize);
		skyboxMaterial.SetFloat("_StarBrightness", starBrightness);
		skyboxMaterial.SetFloat("_StarTwinkle", starTwinkleAmount);
		skyboxMaterial.SetFloat("_StarTwinkleSpeed", starTwinkleSpeed);
		skyboxMaterial.SetFloat("_LargeStarPercentage", largeStarPercentage);

		skyboxMaterial.SetFloat("_MoonPhase", moonPhase);

		skyboxMaterial.SetFloat("_LowCloudCoverage", lowCloudCoverage);
		skyboxMaterial.SetFloat("_LowCloudDensity", lowCloudDensity);
		skyboxMaterial.SetVector("_LowCloudOffset", lowCloudOffset);
		skyboxMaterial.SetColor("_LowCloudColor", lowCloudColor);

		skyboxMaterial.SetFloat("_HighCloudCoverage", highCloudCoverage);
		skyboxMaterial.SetFloat("_HighCloudDensity", highCloudDensity);
		skyboxMaterial.SetVector("_HighCloudOffset", highCloudOffset);
		skyboxMaterial.SetColor("_HighCloudColor", highCloudColor);

		skyboxMaterial.SetFloat("_StormCloudCoverage", stormCloudCoverage);
		skyboxMaterial.SetFloat("_StormCloudDensity", stormCloudDensity);
		skyboxMaterial.SetVector("_StormCloudOffset", stormCloudOffset);
		skyboxMaterial.SetColor("_StormCloudColor", stormCloudColor);

		skyboxMaterial.SetFloat("_LightningIntensity", currentLightningIntensity);
		skyboxMaterial.SetColor("_LightningColor", lightningColor);

		skyboxMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);
		skyboxMaterial.SetFloat("_RayleighScattering", rayleighScattering);
		skyboxMaterial.SetFloat("_MieScattering", mieScattering);
		skyboxMaterial.SetFloat("_CloudsBelowHorizon", cloudsBelowHorizon);

		// Atmospheric fog (skybox-based, not Unity's global fog)
		skyboxMaterial.SetFloat("_FogDensity", fogDensity);
		skyboxMaterial.SetColor("_FogColor", fogColor);
		skyboxMaterial.SetFloat("_FogHeight", fogHeight);
		skyboxMaterial.SetFloat("_FogFalloff", fogFalloff);
		// TEMPORARY TEST - Remove after confirming rotation works
//skyboxMaterial.SetFloat("_MoonRotation", Time.time * 30f); // 30 degrees per second
		Debug.Log("Setting moon rotation to: " + moonRotation +
		  ", Material received: " + skyboxMaterial.GetFloat("_MoonRotation"));
		DynamicGI.UpdateEnvironment();
	}

	void UpdateLighting()
	{
		if (directionalLight == null)
		{
			// Try to find the light again
			Light[] lights = FindObjectsOfType<Light>();
			foreach (Light light in lights)
			{
				if (light.type == LightType.Directional)
				{
					directionalLight = light;
					break;
				}
			}

			if (directionalLight == null)
				return;
		}

		// Validate sunrise/sunset times
		if (sunsetTime <= sunriseTime)
			sunsetTime = sunriseTime + 0.1f;

		// Calculate sun position for lighting
		float sunDayDuration = sunsetTime - sunriseTime;

		if (timeOfDay >= sunriseTime && timeOfDay <= sunsetTime)
		{
			float sunProgress = (timeOfDay - sunriseTime) / sunDayDuration;
			float sunAngle = sunProgress * 180f;
			directionalLight.transform.rotation = Quaternion.Euler(-sunAngle, sunPathRotation, 0);
		}
		else
		{
			directionalLight.transform.rotation = Quaternion.Euler(90, 0, 0);
		}

		// Adjust light intensity based on sun position
		float lightIntensity = 0f;

		if (timeOfDay >= sunriseTime && timeOfDay <= sunsetTime)
		{
			float sunProgress = (timeOfDay - sunriseTime) / sunDayDuration;

			if (sunProgress < 0.08f)
				lightIntensity = Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, sunProgress / 0.08f);
			else if (sunProgress > 0.92f)
				lightIntensity = Mathf.Lerp(dayAmbientIntensity, nightAmbientIntensity, (sunProgress - 0.92f) / 0.08f);
			else
				lightIntensity = dayAmbientIntensity;
		}
		else
		{
			lightIntensity = nightAmbientIntensity;
		}

		lightIntensity *= (1f - stormCloudCoverage * 0.7f);
		lightIntensity *= terrainLightMultiplier;
		lightIntensity += currentLightningIntensity;

		directionalLight.intensity = lightIntensity;

		Color lightColor = Color.white;

		if (timeOfDay >= sunriseTime && timeOfDay <= sunsetTime)
		{
			float sunProgress = (timeOfDay - sunriseTime) / sunDayDuration;

			if (sunProgress < 0.08f)
				lightColor = Color.Lerp(new Color(0.5f, 0.6f, 0.8f), sunColor, sunProgress / 0.08f);
			else if (sunProgress > 0.92f)
				lightColor = Color.Lerp(sunColor, new Color(1f, 0.6f, 0.4f), (sunProgress - 0.92f) / 0.08f);
			else
				lightColor = sunColor;
		}
		else
		{
			lightColor = new Color(0.5f, 0.6f, 0.8f);
		}

		if (lightningActive)
			lightColor = Color.Lerp(lightColor, lightningColor, currentLightningIntensity / lightningIntensity);

		directionalLight.color = lightColor;

		RenderSettings.ambientIntensity = lightIntensity * 0.5f;
		RenderSettings.ambientLight = lightColor;

		// Disable Unity's built-in fog - we're using atmospheric fog in the skybox instead
		RenderSettings.fog = false;
	}
}
