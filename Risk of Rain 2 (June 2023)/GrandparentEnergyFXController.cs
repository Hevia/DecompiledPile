using System.Collections.Generic;
using RoR2;
using UnityEngine;

public class GrandparentEnergyFXController : MonoBehaviour
{
	public List<ParticleSystem> energyFXParticles = new List<ParticleSystem>();

	[HideInInspector]
	public GameObject portalObject;

	private bool isPortalSoundPlaying;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)portalObject != (Object)null))
		{
			return;
		}
		if (!isPortalSoundPlaying)
		{
			if (portalObject.transform.localScale == Vector3.zero)
			{
				Util.PlaySound("Play_grandparent_portal_loop", portalObject);
				isPortalSoundPlaying = false;
			}
		}
		else if (portalObject.transform.localScale != Vector3.zero)
		{
			Util.PlaySound("Stop_grandparent_portal_loop", portalObject);
			isPortalSoundPlaying = true;
		}
	}

	public void TurnOffFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < energyFXParticles.Count; i++)
		{
			EmissionModule emission = energyFXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = false;
		}
		if ((Object)(object)portalObject != (Object)null)
		{
			ParticleSystem componentInChildren = portalObject.GetComponentInChildren<ParticleSystem>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				EmissionModule emission2 = componentInChildren.emission;
				((EmissionModule)(ref emission2)).enabled = false;
			}
		}
	}

	public void TurnOnFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < energyFXParticles.Count; i++)
		{
			EmissionModule emission = energyFXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = true;
		}
		if ((Object)(object)portalObject != (Object)null)
		{
			ParticleSystem componentInChildren = portalObject.GetComponentInChildren<ParticleSystem>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				EmissionModule emission2 = componentInChildren.emission;
				((EmissionModule)(ref emission2)).enabled = true;
			}
		}
	}
}
