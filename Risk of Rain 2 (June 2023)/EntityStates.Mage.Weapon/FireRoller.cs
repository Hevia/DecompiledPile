using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public class FireRoller : BaseState
{
	public static GameObject fireProjectilePrefab;

	public static GameObject iceProjectilePrefab;

	public static GameObject lightningProjectilePrefab;

	public static GameObject fireMuzzleflashEffectPrefab;

	public static GameObject iceMuzzleflashEffectPrefab;

	public static GameObject lightningMuzzleflashEffectPrefab;

	public static GameObject fireAreaIndicatorPrefab;

	public static GameObject iceAreaIndicatorPrefab;

	public static GameObject lightningAreaIndicatorPrefab;

	public static string fireAttackSoundString;

	public static string iceAttackSoundString;

	public static string lightningAttackSoundString;

	public static float targetProjectileSpeed;

	public static float baseEntryDuration = 2f;

	public static float baseDuration = 2f;

	public static float baseExitDuration = 2f;

	public static float fireDamageCoefficient;

	public static float iceDamageCoefficient;

	public static float lightningDamageCoefficient;

	private float stopwatch;

	private float fireDuration;

	private float entryDuration;

	private float exitDuration;

	private bool hasFiredRoller;

	private bool hasBegunExit;

	private GameObject areaIndicatorInstance;

	private string muzzleString;

	private Transform muzzleTransform;

	private Animator animator;

	private ChildLocator childLocator;

	private GameObject areaIndicatorPrefab;

	private float damageCoefficient = 1.2f;

	private string attackString;

	private GameObject projectilePrefab;

	private GameObject muzzleflashEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		InitElement(MageElement.Ice);
		stopwatch = 0f;
		entryDuration = baseEntryDuration / attackSpeedStat;
		fireDuration = baseDuration / attackSpeedStat;
		exitDuration = baseExitDuration / attackSpeedStat;
		Util.PlaySound(attackString, base.gameObject);
		base.characterBody.SetAimTimer(fireDuration + entryDuration + exitDuration + 2f);
		animator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)animator))
		{
			childLocator = ((Component)animator).GetComponent<ChildLocator>();
		}
		muzzleString = "MuzzleRight";
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			muzzleTransform = childLocator.FindChild(muzzleString);
		}
		PlayAnimation("Gesture Left, Additive", "Empty");
		PlayAnimation("Gesture Right, Additive", "Empty");
		PlayAnimation("Gesture, Additive", "EnterRoller", "EnterRoller.playbackRate", entryDuration);
		if (Object.op_Implicit((Object)(object)areaIndicatorPrefab))
		{
			areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab);
		}
	}

	private void UpdateAreaIndicator()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			float num = 1000f;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(GetAimRay(), ref val, num, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				areaIndicatorInstance.transform.position = ((RaycastHit)(ref val)).point;
				areaIndicatorInstance.transform.rotation = Util.QuaternionSafeLookRotation(base.transform.position - areaIndicatorInstance.transform.position, ((RaycastHit)(ref val)).normal);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateAreaIndicator();
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			EntityState.Destroy((Object)(object)areaIndicatorInstance);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= entryDuration && !hasFiredRoller)
		{
			PlayAnimation("Gesture, Additive", "FireRoller", "FireRoller.playbackRate", fireDuration);
			FireRollerProjectile();
			EntityState.Destroy((Object)(object)areaIndicatorInstance);
		}
		if (stopwatch >= entryDuration + fireDuration && !hasBegunExit)
		{
			hasBegunExit = true;
			PlayAnimation("Gesture, Additive", "ExitRoller", "ExitRoller.playbackRate", exitDuration);
		}
		if (stopwatch >= entryDuration + fireDuration + exitDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireRollerProjectile()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		hasFiredRoller = true;
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (!base.isAuthority || !((Object)(object)projectilePrefab != (Object)null))
		{
			return;
		}
		float num = 1000f;
		Ray aimRay = GetAimRay();
		Vector3 forward = aimRay.direction;
		Vector3 val = aimRay.origin;
		float magnitude = targetProjectileSpeed;
		if (Object.op_Implicit((Object)(object)muzzleTransform))
		{
			val = muzzleTransform.position;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(aimRay, ref val2, num, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				float num2 = magnitude;
				Vector3 val3 = ((RaycastHit)(ref val2)).point - val;
				Vector2 val4 = default(Vector2);
				((Vector2)(ref val4))._002Ector(val3.x, val3.z);
				float magnitude2 = ((Vector2)(ref val4)).magnitude;
				float num3 = Trajectory.CalculateInitialYSpeed(magnitude2 / num2, val3.y);
				Vector3 val5 = default(Vector3);
				((Vector3)(ref val5))._002Ector(val4.x / magnitude2 * num2, num3, val4.y / magnitude2 * num2);
				magnitude = ((Vector3)(ref val5)).magnitude;
				forward = val5 / magnitude;
			}
		}
		ProjectileManager.instance.FireProjectile(projectilePrefab, val, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
	}

	private void InitElement(MageElement defaultElement)
	{
		MageCalibrationController component = GetComponent<MageCalibrationController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			MageElement activeCalibrationElement = component.GetActiveCalibrationElement();
			if (activeCalibrationElement != 0)
			{
				defaultElement = activeCalibrationElement;
			}
		}
		switch (defaultElement)
		{
		case MageElement.Fire:
			damageCoefficient = fireDamageCoefficient;
			attackString = fireAttackSoundString;
			projectilePrefab = fireProjectilePrefab;
			muzzleflashEffectPrefab = fireMuzzleflashEffectPrefab;
			areaIndicatorPrefab = fireAreaIndicatorPrefab;
			break;
		case MageElement.Ice:
			damageCoefficient = iceDamageCoefficient;
			attackString = iceAttackSoundString;
			projectilePrefab = iceProjectilePrefab;
			muzzleflashEffectPrefab = iceMuzzleflashEffectPrefab;
			areaIndicatorPrefab = iceAreaIndicatorPrefab;
			break;
		case MageElement.Lightning:
			damageCoefficient = lightningDamageCoefficient;
			attackString = lightningAttackSoundString;
			projectilePrefab = lightningProjectilePrefab;
			muzzleflashEffectPrefab = lightningMuzzleflashEffectPrefab;
			areaIndicatorPrefab = lightningAreaIndicatorPrefab;
			break;
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
