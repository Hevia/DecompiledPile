using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.JellyfishMonster;

public class DeathState : GenericCharacterDeath
{
	public static GameObject enterEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		DestroyModel();
		if (NetworkServer.active)
		{
			DestroyBodyAsapServer();
		}
	}

	protected override void CreateDeathEffects()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.CreateDeathEffects();
		if (Object.op_Implicit((Object)(object)enterEffectPrefab))
		{
			EffectManager.SimpleEffect(enterEffectPrefab, base.transform.position, base.transform.rotation, transmit: false);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
