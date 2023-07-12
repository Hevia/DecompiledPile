using RoR2;
using UnityEngine;

namespace EntityStates.AffixEarthHealer;

public class Chargeup : BaseState
{
	public static float duration;

	public static string enterSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		((Component)FindModelChild("ChargeUpFX")).gameObject.SetActive(true);
		PlayAnimation("Base", "ChargeUp", "ChargeUp.playbackRate", duration);
		Util.PlaySound(enterSoundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new Heal());
		}
	}

	public override void OnExit()
	{
		((Component)FindModelChild("ChargeUpFX")).gameObject.SetActive(false);
		base.OnExit();
	}
}
