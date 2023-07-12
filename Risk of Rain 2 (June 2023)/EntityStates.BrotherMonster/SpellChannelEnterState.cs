using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class SpellChannelEnterState : SpellBaseState
{
	public static GameObject channelBeginEffectPrefab;

	public static float duration;

	private Transform trueDeathEffect;

	protected override bool DisplayWeapon => false;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "SpellChannelEnter", "SpellChannelEnter.playbackRate", duration);
		Util.PlaySound("Play_moonBrother_phase4_transition", base.gameObject);
		trueDeathEffect = FindModelChild("TrueDeathEffect");
		if (Object.op_Implicit((Object)(object)trueDeathEffect))
		{
			((Component)trueDeathEffect).gameObject.SetActive(true);
			((Component)trueDeathEffect).GetComponent<ScaleParticleSystemDuration>().newDuration = 10f;
		}
		HurtBoxGroup component = ((Component)GetModelTransform()).GetComponent<HurtBoxGroup>();
		if (Object.op_Implicit((Object)(object)component))
		{
			int hurtBoxesDeactivatorCounter = component.hurtBoxesDeactivatorCounter + 1;
			component.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > duration)
		{
			outer.SetNextState(new SpellChannelState());
		}
	}

	public override void OnExit()
	{
		HurtBoxGroup component = ((Component)GetModelTransform()).GetComponent<HurtBoxGroup>();
		if (Object.op_Implicit((Object)(object)component))
		{
			int hurtBoxesDeactivatorCounter = component.hurtBoxesDeactivatorCounter - 1;
			component.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		if (Object.op_Implicit((Object)(object)channelBeginEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(channelBeginEffectPrefab, base.gameObject, "SpellChannel", transmit: false);
		}
		if (Object.op_Implicit((Object)(object)trueDeathEffect))
		{
			((Component)trueDeathEffect).gameObject.SetActive(false);
		}
		base.OnExit();
	}
}
