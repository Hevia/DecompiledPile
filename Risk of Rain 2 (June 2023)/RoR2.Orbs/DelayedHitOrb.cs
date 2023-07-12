using UnityEngine;

namespace RoR2.Orbs;

public class DelayedHitOrb : GenericDamageOrb
{
	public float delay;

	public GameObject delayedEffectPrefab;

	public GameObject orbEffect;

	public override void Begin()
	{
		base.Begin();
		base.duration = delay;
	}

	protected override GameObject GetOrbEffect()
	{
		return orbEffect;
	}

	public override void OnArrival()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target) && Object.op_Implicit((Object)(object)((Component)target).transform))
		{
			EffectManager.SpawnEffect(delayedEffectPrefab, new EffectData
			{
				origin = ((Component)target).transform.position
			}, transmit: true);
			base.OnArrival();
		}
	}
}
