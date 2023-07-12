using RoR2;
using UnityEngine;

namespace EntityStates.Barrel;

public class Opening : EntityState
{
	public static float duration = 1f;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Opening", "Opening.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)base.sfxLocator))
		{
			Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new Opened());
		}
	}
}
