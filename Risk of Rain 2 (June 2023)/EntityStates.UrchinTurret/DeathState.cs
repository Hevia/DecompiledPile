using RoR2;
using UnityEngine;

namespace EntityStates.UrchinTurret;

public class DeathState : BaseState
{
	public static GameObject initialExplosion;

	public static float effectScale;

	public static string deathString;

	public override void OnEnter()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(deathString, base.gameObject);
		Transform val = FindModelChild("Muzzle");
		if (base.isAuthority)
		{
			if (Object.op_Implicit((Object)(object)initialExplosion))
			{
				EffectManager.SpawnEffect(initialExplosion, new EffectData
				{
					origin = val.position,
					scale = effectScale
				}, transmit: true);
			}
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
