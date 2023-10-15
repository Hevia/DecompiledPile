using UnityEngine;

namespace EntityStates.VoidJailer.Weapon;

public class Fire : GenericProjectileBaseState
{
	public static string animationLayerName;

	public static string animationStateName;

	public static string animationPlaybackRateName;

	public static int totalProjectileCount;

	public static float maxRandomDistance;

	public static float basePriorityReductionDuration;

	private float priorityReductionDuration;

	private Transform muzzleTransform;

	public override void OnEnter()
	{
		muzzleTransform = FindModelChild("ClawMuzzle");
		base.OnEnter();
		base.characterBody.SetAimTimer(duration + 3f);
		for (int i = 1; i < totalProjectileCount; i++)
		{
			FireProjectile();
		}
		priorityReductionDuration = basePriorityReductionDuration / attackSpeedStat;
	}

	protected override Ray ModifyProjectileAimRay(Ray aimRay)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		aimRay.origin = aimRay.origin + Random.insideUnitSphere * maxRandomDistance;
		return aimRay;
	}

	protected override void PlayAnimation(float duration)
	{
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateName, duration);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!(base.fixedAge > priorityReductionDuration))
		{
			return InterruptPriority.PrioritySkill;
		}
		return InterruptPriority.Skill;
	}
}
