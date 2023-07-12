using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidBarnacle;

public class DeathState : GenericCharacterDeath
{
	public static string animationLayerName;

	public static string animationStateName;

	public static string animationPlaybackRateName;

	public static float duration;

	public static GameObject deathFXPrefab;

	public override void OnEnter()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if ((Object)(object)deathFXPrefab != (Object)null)
		{
			EffectManager.SimpleEffect(deathFXPrefab, base.transform.position, base.transform.rotation, transmit: true);
		}
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateName, duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && NetworkServer.active)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
