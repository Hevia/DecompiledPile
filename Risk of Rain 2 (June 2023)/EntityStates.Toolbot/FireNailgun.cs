using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class FireNailgun : BaseNailgunState
{
	public static float baseRefireInterval;

	public static string spinUpSound;

	private float refireStopwatch;

	private uint loopSoundID;

	protected override float GetBaseDuration()
	{
		return baseRefireInterval;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		loopSoundID = Util.PlaySound(spinUpSound, base.gameObject);
		base.animateNailgunFiring = true;
		refireStopwatch = duration;
	}

	public override void FixedUpdate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		refireStopwatch += Time.fixedDeltaTime;
		if (refireStopwatch >= duration)
		{
			PullCurrentStats();
			refireStopwatch -= duration;
			Ray aimRay = GetAimRay();
			Vector3 direction = aimRay.direction;
			Vector3 val = Vector3.Cross(Vector3.up, direction);
			float num = Mathf.Sin((float)fireNumber * 0.5f);
			Vector3 val2 = Quaternion.AngleAxis(base.characterBody.spreadBloomAngle * num, val) * direction;
			val2 = Quaternion.AngleAxis((float)fireNumber * -65.454544f, direction) * val2;
			aimRay.direction = val2;
			FireBullet(aimRay, 1, 0f, 0f);
		}
		if (base.isAuthority && (!IsKeyDownAuthority() || base.characterBody.isSprinting))
		{
			outer.SetNextState(new NailgunSpinDown
			{
				activatorSkillSlot = base.activatorSkillSlot
			});
		}
	}

	public override void OnExit()
	{
		base.animateNailgunFiring = false;
		AkSoundEngine.StopPlayingID(loopSoundID);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
