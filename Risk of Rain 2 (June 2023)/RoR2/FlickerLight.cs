using System;
using UnityEngine;

namespace RoR2;

public class FlickerLight : MonoBehaviour
{
	public Light light;

	public Wave[] sinWaves;

	private float initialLightIntensity;

	private float stopwatch;

	private float randomPhase;

	private void Start()
	{
		initialLightIntensity = light.intensity;
		randomPhase = Random.Range(0f, MathF.PI * 2f);
		for (int i = 0; i < sinWaves.Length; i++)
		{
			sinWaves[i].cycleOffset += randomPhase;
		}
	}

	private void Update()
	{
		stopwatch += Time.deltaTime;
		float num = initialLightIntensity;
		for (int i = 0; i < sinWaves.Length; i++)
		{
			num *= 1f + sinWaves[i].Evaluate(stopwatch);
		}
		light.intensity = num;
	}
}
