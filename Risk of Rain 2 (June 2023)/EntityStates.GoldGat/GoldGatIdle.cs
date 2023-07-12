using RoR2;
using UnityEngine;

namespace EntityStates.GoldGat;

public class GoldGatIdle : BaseGoldGatState
{
	public static string windDownSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(windDownSoundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)gunAnimator))
		{
			gunAnimator.SetFloat("Crank.playbackRate", 0f, 1f, Time.fixedDeltaTime);
		}
		if (base.isAuthority && shouldFire && bodyMaster.money != 0 && bodyEquipmentSlot.stock > 0)
		{
			outer.SetNextState(new GoldGatFire
			{
				shouldFire = shouldFire
			});
		}
	}
}
