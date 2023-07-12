using System.Collections.Generic;
using UnityEngine;

public class LunarWispFXController : MonoBehaviour
{
	public List<ParticleSystem> FXParticles = new List<ParticleSystem>();

	private void Start()
	{
	}

	private void FixedUpdate()
	{
	}

	public void TurnOffFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < FXParticles.Count; i++)
		{
			EmissionModule emission = FXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = false;
		}
	}

	public void TurnOnFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < FXParticles.Count; i++)
		{
			EmissionModule emission = FXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = true;
		}
	}
}
