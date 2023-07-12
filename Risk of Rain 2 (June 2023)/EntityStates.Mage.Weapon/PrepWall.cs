using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public class PrepWall : BaseState
{
	public static float baseDuration;

	public static GameObject areaIndicatorPrefab;

	public static GameObject projectilePrefab;

	public static float damageCoefficient;

	public static GameObject muzzleflashEffect;

	public static GameObject goodCrosshairPrefab;

	public static GameObject badCrosshairPrefab;

	public static string prepWallSoundString;

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
		base.characterBody.SetAimTimer(duration + 2f);
		PlayAnimation("Gesture, Additive", "PrepWall", "PrepWall.playbackRate", duration);
		Util.PlaySound(prepWallSoundString, base.gameObject);
		areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab);
		UpdateAreaIndicator();
	}

	private void UpdateAreaIndicator()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		bool flag = goodPlacement;
		goodPlacement = false;
		areaIndicatorInstance.SetActive(true);
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			float num = maxDistance;
			float extraRaycastDistance = 0f;
			Ray aimRay = GetAimRay();
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(aimRay, base.gameObject, out extraRaycastDistance), ref val, num + extraRaycastDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				areaIndicatorInstance.transform.position = ((RaycastHit)(ref val)).point;
				areaIndicatorInstance.transform.up = ((RaycastHit)(ref val)).normal;
				areaIndicatorInstance.transform.forward = -((Ray)(ref aimRay)).direction;
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
		if (stopwatch >= duration && !base.inputBank.skill3.down && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if (!outer.destroying)
		{
			if (goodPlacement)
			{
				PlayAnimation("Gesture, Additive", "FireWall");
				Util.PlaySound(fireSoundString, base.gameObject);
				if (Object.op_Implicit((Object)(object)areaIndicatorInstance) && base.isAuthority)
				{
					EffectManager.SimpleMuzzleFlash(muzzleflashEffect, base.gameObject, "MuzzleLeft", transmit: true);
					EffectManager.SimpleMuzzleFlash(muzzleflashEffect, base.gameObject, "MuzzleRight", transmit: true);
					Vector3 forward = areaIndicatorInstance.transform.forward;
					forward.y = 0f;
					((Vector3)(ref forward)).Normalize();
					Vector3 val = Vector3.Cross(Vector3.up, forward);
					bool crit = Util.CheckRoll(critStat, base.characterBody.master);
					ProjectileManager.instance.FireProjectile(projectilePrefab, areaIndicatorInstance.transform.position + Vector3.up, Util.QuaternionSafeLookRotation(val), base.gameObject, damageStat * damageCoefficient, 0f, crit);
					ProjectileManager.instance.FireProjectile(projectilePrefab, areaIndicatorInstance.transform.position + Vector3.up, Util.QuaternionSafeLookRotation(-val), base.gameObject, damageStat * damageCoefficient, 0f, crit);
				}
			}
			else
			{
				base.skillLocator.utility.AddOneStock();
				PlayCrossfade("Gesture, Additive", "BufferEmpty", 0.2f);
			}
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
