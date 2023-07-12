using RoR2;
using UnityEngine;

namespace EntityStates.VagrantMonster;

public class DeathState : BaseState
{
	public static GameObject initialExplosion;

	public static string deathString;

	public override void OnEnter()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
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
		if (base.isAuthority && Object.op_Implicit((Object)(object)initialExplosion))
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
