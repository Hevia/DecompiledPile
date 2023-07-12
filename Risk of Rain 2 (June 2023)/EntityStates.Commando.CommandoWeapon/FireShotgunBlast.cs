using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class FireShotgunBlast : GenericBulletBaseState
{
	public static float delayBetweenShotgunBlasts;

	private bool hasFiredSecondBlast;

	public override void OnEnter()
	{
		muzzleName = "MuzzleLeft";
		base.OnEnter();
		PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
		PlayAnimation("Gesture Override, Left", "FirePistol, Left");
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!hasFiredSecondBlast && delayBetweenShotgunBlasts / attackSpeedStat < base.fixedAge)
		{
			hasFiredSecondBlast = true;
			PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
			PlayAnimation("Gesture Override, Right", "FirePistol, Right");
			muzzleName = "MuzzleRight";
			FireBullet(GetAimRay());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
