using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class EnterSkyLeap : BaseState
{
	public static float baseDuration;

	public static string soundString;

	private float duration;

	public override void OnEnter()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(soundString, base.gameObject);
		PlayAnimation("Body", "EnterSkyLeap", "SkyLeap.playbackRate", duration);
		PlayAnimation("FullBody Override", "BufferEmpty");
		base.characterDirection.moveVector = base.characterDirection.forward;
		base.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, baseDuration);
		AimAnimator aimAnimator = GetAimAnimator();
		if (Object.op_Implicit((Object)(object)aimAnimator))
		{
			((Behaviour)aimAnimator).enabled = true;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > duration)
		{
			outer.SetNextState(new HoldSkyLeap());
		}
	}
}
