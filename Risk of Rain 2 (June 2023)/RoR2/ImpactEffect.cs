using System;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(EffectComponent))]
internal class ImpactEffect : MonoBehaviour
{
	public ParticleSystem[] particleSystems = Array.Empty<ParticleSystem>();

	private void Start()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		EffectComponent component = ((Component)this).GetComponent<EffectComponent>();
		Color val = ((component.effectData != null) ? Color32.op_Implicit(component.effectData.color) : Color.white);
		for (int i = 0; i < particleSystems.Length; i++)
		{
			MainModule main = particleSystems[i].main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(val);
			particleSystems[i].Play();
		}
	}
}
