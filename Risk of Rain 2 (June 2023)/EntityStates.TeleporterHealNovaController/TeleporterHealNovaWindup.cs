using RoR2;
using UnityEngine;

namespace EntityStates.TeleporterHealNovaController;

public class TeleporterHealNovaWindup : BaseState
{
	public static GameObject chargeEffectPrefab;

	public static float duration;

	public override void OnEnter()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		EffectManager.SimpleEffect(chargeEffectPrefab, base.transform.position, Quaternion.identity, transmit: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && duration <= base.fixedAge)
		{
			outer.SetNextState(new TeleporterHealNovaPulse());
		}
	}
}
