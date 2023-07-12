using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BeetleQueenMonster;

public class BeetleWardDeath : BaseState
{
	public static GameObject initialExplosion;

	public static string deathString;

	public override void OnEnter()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(deathString, base.gameObject);
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
		if (Object.op_Implicit((Object)(object)initialExplosion) && NetworkServer.active)
		{
			EffectManager.SimpleImpactEffect(initialExplosion, base.transform.position, Vector3.up, transmit: true);
		}
		EntityState.Destroy((Object)(object)base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
