using System;
using System.Collections.Generic;
using UnityEngine;

public class RootJungleFXController : MonoBehaviour
{
	public int minSystemsActive = 2;

	public int maxSystemsActive = 4;

	public float minActiveTime = 10f;

	public float maxActiveTime = 30f;

	private int numActive;

	private float timeActive;

	private float effectsTimer;

	private bool timeFX;

	public List<ParticleSystem> FXParticles = new List<ParticleSystem>();

	private void Start()
	{
		TurnOffFX(FXParticles.Count);
	}

	private void FixedUpdate()
	{
		if (timeFX)
		{
			effectsTimer -= Time.fixedDeltaTime;
			if (effectsTimer <= 0f)
			{
				timeFX = false;
				TurnOffFX(numActive);
			}
		}
	}

	public void SetupParticles()
	{
		numActive = Random.Range(minSystemsActive, maxSystemsActive + 1);
		timeActive = Random.Range(minActiveTime, maxActiveTime);
		effectsTimer = timeActive;
		FXParticles = Shuffle(FXParticles);
		TurnOnFX(numActive);
		timeFX = true;
	}

	public void TurnOffFX(int amount)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < amount; i++)
		{
			EmissionModule emission = FXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = false;
		}
		SetupParticles();
	}

	public void TurnOnFX(int amount)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < amount; i++)
		{
			EmissionModule emission = FXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = true;
		}
	}

	public static List<T> Shuffle<T>(List<T> list)
	{
		Random random = new Random();
		for (int i = 0; i < list.Count; i++)
		{
			int index = random.Next(0, i);
			T value = list[index];
			list[index] = list[i];
			list[i] = value;
		}
		return list;
	}
}
