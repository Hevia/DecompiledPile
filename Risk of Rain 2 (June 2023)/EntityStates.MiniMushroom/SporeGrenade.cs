using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.MiniMushroom;

public class SporeGrenade : BaseState
{
	public static GameObject chargeEffectPrefab;

	public static string attackSoundString;

	public static string chargeUpSoundString;

	public static float recoilAmplitude = 1f;

	public static GameObject projectilePrefab;

	public static float baseDuration = 2f;

	public static string muzzleString;

	public static float damageCoefficient;

	public static float timeToTarget = 3f;

	public static float projectileVelocity = 55f;

	public static float minimumDistance;

	public static float maximumDistance;

	public static float baseChargeTime = 2f;

	private uint chargeupSoundID;

	private Ray projectileRay;

	private Transform modelTransform;

	private float duration;

	private float chargeTime;

	private bool hasFired;

	private Animator modelAnimator;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		chargeTime = baseChargeTime / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool("isCharged", false);
			PlayAnimation("Gesture, Additive", "Charge");
			chargeupSoundID = Util.PlaySound(chargeUpSoundString, ((Component)base.characterBody.modelLocator.modelTransform).gameObject);
		}
		Transform val = FindModelChild("ChargeSpot");
		if (Object.op_Implicit((Object)(object)val))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!(base.fixedAge >= chargeTime))
		{
			return;
		}
		if (!hasFired)
		{
			hasFired = true;
			Animator obj = modelAnimator;
			if (obj != null)
			{
				obj.SetBool("isCharged", true);
			}
			if (base.isAuthority)
			{
				FireGrenade(muzzleString);
			}
		}
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		PlayAnimation("Gesture, Additive", "Empty");
		AkSoundEngine.StopPlayingID(chargeupSoundID);
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		base.OnExit();
	}

	private void FireGrenade(string targetMuzzle)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Ray val = default(Ray);
		((Ray)(ref val))._002Ector(aimRay.origin, Vector3.up);
		Transform val2 = FindModelChild(targetMuzzle);
		if (Object.op_Implicit((Object)(object)val2))
		{
			((Ray)(ref val)).origin = val2.position;
		}
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = aimRay.direction;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			bullseyeSearch.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
		}
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.RefreshCandidates();
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		bool flag = false;
		Vector3 val3 = Vector3.zero;
		RaycastHit val4 = default(RaycastHit);
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			val3 = ((Component)hurtBox).transform.position;
			flag = true;
		}
		else if (Physics.Raycast(aimRay, ref val4, 1000f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)1))
		{
			val3 = ((RaycastHit)(ref val4)).point;
			flag = true;
		}
		float magnitude = projectileVelocity;
		if (flag)
		{
			Vector3 val5 = val3 - ((Ray)(ref val)).origin;
			Vector2 val6 = default(Vector2);
			((Vector2)(ref val6))._002Ector(val5.x, val5.z);
			float magnitude2 = ((Vector2)(ref val6)).magnitude;
			Vector2 val7 = val6 / magnitude2;
			if (magnitude2 < minimumDistance)
			{
				magnitude2 = minimumDistance;
			}
			if (magnitude2 > maximumDistance)
			{
				magnitude2 = maximumDistance;
			}
			float num = Trajectory.CalculateInitialYSpeed(timeToTarget, val5.y);
			float num2 = magnitude2 / timeToTarget;
			Vector3 direction = default(Vector3);
			((Vector3)(ref direction))._002Ector(val7.x * num2, num, val7.y * num2);
			magnitude = ((Vector3)(ref direction)).magnitude;
			((Ray)(ref val)).direction = direction;
		}
		Quaternion rotation = Util.QuaternionSafeLookRotation(((Ray)(ref val)).direction + Random.insideUnitSphere * 0.05f);
		ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref val)).origin, rotation, base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
	}
}
