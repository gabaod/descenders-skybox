using UnityEngine;
using ModTool.Interface;

public class LightningAudioTrigger : ModBehaviour
{
	public Material skyboxMaterial;
	public AudioSource audioSource;

	// Individual clips instead of array
	public AudioClip thunderClip1;
	public AudioClip thunderClip2;
	public AudioClip thunderClip3;
	public AudioClip thunderClip4;
	public AudioClip thunderClip5;

	[Header("Audio Settings")]
	[Range(0f, 1f)]
	public float volumeScale = 1f;
	public bool randomizeDelay = true;
	[Range(0f, 3f)]
	public float minDelay = 0.1f;
	[Range(0f, 5f)]
	public float maxDelay = 2f;

	private float previousLightningIntensity = 0f;
	private float thunderCooldown = 0f;
	private AudioClip[] thunderClips;

	void Start()
	{
		// Build array from individual clips (ignore nulls)
		System.Collections.Generic.List<AudioClip> clips = new System.Collections.Generic.List<AudioClip>();
		if (thunderClip1 != null) clips.Add(thunderClip1);
		if (thunderClip2 != null) clips.Add(thunderClip2);
		if (thunderClip3 != null) clips.Add(thunderClip3);
		if (thunderClip4 != null) clips.Add(thunderClip4);
		if (thunderClip5 != null) clips.Add(thunderClip5);

		thunderClips = clips.ToArray();
	}

	void Update()
	{
		thunderCooldown -= Time.deltaTime;

		// Get current lightning intensity from shader
		float currentIntensity = skyboxMaterial.GetFloat("_LightningIntensity");

		// Detect when lightning triggers (rising edge detection)
		if (currentIntensity > 0.01f && previousLightningIntensity <= 0.01f && thunderCooldown <= 0f)
		{
			if (randomizeDelay)
			{
				// Random delay for realism (simulates distance)
				float delay = Random.Range(minDelay, maxDelay);
				StartCoroutine(PlayThunderWithDelay(delay));
			}
			else
			{
				PlayThunder();
			}

			// Prevent multiple triggers in quick succession
			thunderCooldown = 0.2f;
		}

		previousLightningIntensity = currentIntensity;
	}

	void PlayThunder()
	{
		if (thunderClips != null && thunderClips.Length > 0)
		{
			// Pick random clip for variety
			AudioClip clip = thunderClips[Random.Range(0, thunderClips.Length)];
			audioSource.PlayOneShot(clip, volumeScale);
		}
	}

	System.Collections.IEnumerator PlayThunderWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		PlayThunder();
	}
}
