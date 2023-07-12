using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class ToolbotDualWieldStart : ToolbotDualWieldBase
{
	public static float baseDuration;

	public static string animLayer;

	public static string animStateName;

	public static string animPlaybackRateParam;

	public static string enterSfx;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animLayer, animStateName, animPlaybackRateParam, duration);
		Util.PlaySound(enterSfx, base.gameObject);
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor) && !base.characterMotor.isGrounded)
		{
			base.characterMotor.disableAirControlUntilCollision = true;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new ToolbotDualWield
			{
				activatorSkillSlot = base.activatorSkillSlot
			});
		}
	}
}
