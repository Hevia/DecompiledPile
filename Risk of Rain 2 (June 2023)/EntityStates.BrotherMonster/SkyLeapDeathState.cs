using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class SkyLeapDeathState : GenericCharacterDeath
{
	public static float baseDuration;

	private float duration;

	protected override bool shouldAutoDestroy => false;

	public override void OnEnter()
	{
		duration = baseDuration / attackSpeedStat;
		base.OnEnter();
	}

	protected override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		PlayAnimation("Body", "EnterSkyLeap", "SkyLeap.playbackRate", duration);
		PlayAnimation("FullBody Override", "BufferEmpty");
		base.characterDirection.moveVector = base.characterDirection.forward;
		AimAnimator aimAnimator = GetAimAnimator();
		if (Object.op_Implicit((Object)(object)aimAnimator))
		{
			((Behaviour)aimAnimator).enabled = true;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			DestroyBodyAsapServer();
		}
	}
}
