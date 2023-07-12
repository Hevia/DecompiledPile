using System.Collections.Generic;
using UnityEngine;

public class ParentEnergyFXController : MonoBehaviour
{
	public List<ParticleSystem> energyFXParticles = new List<ParticleSystem>();

	public ParticleSystem loomingPresenceParticles;

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
		for (int i = 0; i < energyFXParticles.Count; i++)
		{
			EmissionModule emission = energyFXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = false;
		}
	}

	public void TurnOnFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < energyFXParticles.Count; i++)
		{
			EmissionModule emission = energyFXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = true;
		}
	}

	public void SetLoomingPresenceParticles(bool setTo)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		EmissionModule emission = loomingPresenceParticles.emission;
		((EmissionModule)(ref emission)).enabled = setTo;
	}
}
