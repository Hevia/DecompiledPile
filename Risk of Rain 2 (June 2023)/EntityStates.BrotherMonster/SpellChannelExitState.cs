using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class SpellChannelExitState : SpellBaseState
{
	public static float lendInterval;

	public static float duration;

	public static GameObject channelFinishEffectPrefab;

	protected override bool DisplayWeapon => false;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "SpellChannelExit", "SpellChannelExit.playbackRate", duration);
		if (NetworkServer.active && Object.op_Implicit((Object)(object)itemStealController))
		{
			itemStealController.stealInterval = lendInterval;
			itemStealController.LendImmediately(base.characterBody.inventory);
		}
		if (Object.op_Implicit((Object)(object)channelFinishEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(channelFinishEffectPrefab, base.gameObject, "SpellChannel", transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		SetStateOnHurt component = GetComponent<SetStateOnHurt>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.canBeFrozen = true;
		}
		base.OnExit();
	}
}
