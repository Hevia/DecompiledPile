using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Destructible;

public class AltarSkeletonDeath : BaseState
{
	public static GameObject explosionEffectPrefab;

	public static float explosionRadius;

	public static string deathSoundString;

	private float stopwatch;

	public static event Action onDeath;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(deathSoundString, base.gameObject);
		Explode();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	private void Explode()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelBaseTransform).gameObject);
			}
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelTransform).gameObject);
			}
		}
		if (Object.op_Implicit((Object)(object)explosionEffectPrefab) && NetworkServer.active)
		{
			EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
			{
				origin = base.transform.position,
				scale = explosionRadius,
				rotation = Quaternion.identity
			}, transmit: false);
		}
		AltarSkeletonDeath.onDeath?.Invoke();
		EntityState.Destroy((Object)(object)base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
