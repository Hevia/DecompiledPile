using System;
using UnityEngine;

namespace RoR2;

public class OmniEffect : MonoBehaviour
{
	[Serializable]
	public class OmniEffectElement
	{
		public string name;

		public ParticleSystem particleSystem;

		public bool particleSystemEmitParticles;

		public Material particleSystemOverrideMaterial;

		public float maximumValidRadius = float.PositiveInfinity;

		public float bonusEmissionPerBonusRadius;

		public void ProcessEffectElement(float radius, float minimumValidRadius)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)particleSystem))
			{
				return;
			}
			if (Object.op_Implicit((Object)(object)particleSystemOverrideMaterial))
			{
				ParticleSystemRenderer component = ((Component)particleSystem).GetComponent<ParticleSystemRenderer>();
				if (Object.op_Implicit((Object)(object)particleSystemOverrideMaterial))
				{
					((Renderer)component).material = particleSystemOverrideMaterial;
				}
			}
			EmissionModule emission = particleSystem.emission;
			if (((EmissionModule)(ref emission)).burstCount > 0)
			{
				Burst burst = ((EmissionModule)(ref emission)).GetBurst(0);
				int num = ((Burst)(ref burst)).maxCount + (int)((radius - minimumValidRadius) * bonusEmissionPerBonusRadius);
				MainModule main = particleSystem.main;
				int num2 = Mathf.Min(num, ((MainModule)(ref main)).maxParticles);
				((EmissionModule)(ref emission)).SetBurst(0, new Burst(0f, MinMaxCurve.op_Implicit((float)num2)));
			}
			if (particleSystemEmitParticles)
			{
				((Component)particleSystem).gameObject.SetActive(true);
			}
			else
			{
				((Component)particleSystem).gameObject.SetActive(false);
			}
		}
	}

	[Serializable]
	public class OmniEffectGroup
	{
		public string name;

		public OmniEffectElement[] omniEffectElements;
	}

	public OmniEffectGroup[] omniEffectGroups;

	private float radius;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		radius = ((Component)this).transform.localScale.x;
		OmniEffectGroup[] array = omniEffectGroups;
		foreach (OmniEffectGroup omniEffectGroup in array)
		{
			float minimumValidRadius = 0f;
			for (int j = 0; j < omniEffectGroup.omniEffectElements.Length; j++)
			{
				OmniEffectElement omniEffectElement = omniEffectGroup.omniEffectElements[j];
				if (omniEffectElement.maximumValidRadius >= radius)
				{
					omniEffectElement.ProcessEffectElement(radius, minimumValidRadius);
					break;
				}
				minimumValidRadius = omniEffectElement.maximumValidRadius;
			}
		}
	}
}
