using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Treebot.Weapon;

public class CreatePounder : BaseState
{
	public static float baseDuration;

	public static GameObject areaIndicatorPrefab;

	public static GameObject projectilePrefab;

	public static float damageCoefficient;

	public static GameObject muzzleflashEffect;

	public static GameObject goodCrosshairPrefab;

	public static GameObject badCrosshairPrefab;

	public static string prepSoundString;

	public static float maxDistance;

	public static string fireSoundString;

	public static float maxSlopeAngle;

	private float duration;

	private float stopwatch;

	private bool goodPlacement;

	private GameObject areaIndicatorInstance;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "PrepWall", "PrepWall.playbackRate", duration);
		Util.PlaySound(prepSoundString, base.gameObject);
		areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab);
		UpdateAreaIndicator();
	}

	private void UpdateAreaIndicator()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = goodPlacement;
		goodPlacement = false;
		areaIndicatorInstance.SetActive(true);
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			float num = maxDistance;
			float extraRaycastDistance = 0f;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), base.gameObject, out extraRaycastDistance), ref val, num + extraRaycastDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				areaIndicatorInstance.transform.position = ((RaycastHit)(ref val)).point;
				areaIndicatorInstance.transform.up = ((RaycastHit)(ref val)).normal;
				goodPlacement = Vector3.Angle(Vector3.up, ((RaycastHit)(ref val)).normal) < maxSlopeAngle;
			}
			if (flag != goodPlacement || crosshairOverrideRequest == null)
			{
				crosshairOverrideRequest?.Dispose();
				GameObject crosshairPrefab = (goodPlacement ? goodCrosshairPrefab : badCrosshairPrefab);
				crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairPrefab, CrosshairUtils.OverridePriority.Skill);
			}
		}
		areaIndicatorInstance.SetActive(goodPlacement);
	}

	public override void Update()
	{
		base.Update();
		UpdateAreaIndicator();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && !base.inputBank.skill4.down && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (goodPlacement)
		{
			Util.PlaySound(fireSoundString, base.gameObject);
			if (Object.op_Implicit((Object)(object)areaIndicatorInstance) && base.isAuthority)
			{
				bool crit = Util.CheckRoll(critStat, base.characterBody.master);
				ProjectileManager.instance.FireProjectile(projectilePrefab, areaIndicatorInstance.transform.position, Quaternion.identity, base.gameObject, damageStat * damageCoefficient, 0f, crit);
			}
		}
		else
		{
			base.skillLocator.special.AddOneStock();
		}
		EntityState.Destroy((Object)(object)areaIndicatorInstance.gameObject);
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
