using RoR2;
using UnityEngine;

namespace EntityStates.Bandit2.Weapon;

public abstract class Bandit2FirePrimaryBase : GenericBulletBaseState
{
	[SerializeField]
	public float minimumBaseDuration;

	protected float minimumDuration;

	public override void OnEnter()
	{
		minimumDuration = minimumBaseDuration / attackSpeedStat;
		base.OnEnter();
		PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", duration);
	}

	protected override void ModifyBullet(BulletAttack bulletAttack)
	{
		base.ModifyBullet(bulletAttack);
		bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!(base.fixedAge > minimumDuration))
		{
			return InterruptPriority.Skill;
		}
		return InterruptPriority.Any;
	}
}
