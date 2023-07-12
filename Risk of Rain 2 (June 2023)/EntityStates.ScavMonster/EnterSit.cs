using RoR2;
using UnityEngine;

namespace EntityStates.ScavMonster;

public class EnterSit : BaseSitState
{
	public static float baseDuration;

	public static string soundString;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(soundString, base.gameObject);
		PlayCrossfade("Body", "EnterSit", "Sit.playbackRate", duration, 0.1f);
		base.modelLocator.normalizeToFloor = true;
		((Behaviour)((Component)base.modelLocator.modelTransform).GetComponent<AimAnimator>()).enabled = true;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new FindItem());
		}
	}
}
