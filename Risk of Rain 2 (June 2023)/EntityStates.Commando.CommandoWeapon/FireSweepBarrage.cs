using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Commando.CommandoWeapon;

public class FireSweepBarrage : BaseState
{
	public static string enterSound;

	public static string muzzle;

	public static string fireSoundString;

	public static GameObject muzzleEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float baseTotalDuration;

	public static float baseFiringDuration;

	public static float fieldOfView;

	public static float maxDistance;

	public static float damageCoefficient;

	public static float procCoefficient;

	public static float force;

	public static int minimumFireCount;

	public static GameObject impactEffectPrefab;

	private float totalDuration;

	private float firingDuration;

	private int totalBulletsToFire;

	private int totalBulletsFired;

	private int targetHurtboxIndex;

	private float timeBetweenBullets;

	private List<HurtBox> targetHurtboxes = new List<HurtBox>();

	private float fireTimer;

	private ChildLocator childLocator;

	private int muzzleIndex;

	private Transform muzzleTransform;

	public override void OnEnter()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		totalDuration = baseTotalDuration / attackSpeedStat;
		firingDuration = baseFiringDuration / attackSpeedStat;
		base.characterBody.SetAimTimer(3f);
		PlayAnimation("Gesture, Additive", "FireSweepBarrage", "FireSweepBarrage.playbackRate", totalDuration);
		PlayAnimation("Gesture, Override", "FireSweepBarrage", "FireSweepBarrage.playbackRate", totalDuration);
		Util.PlaySound(enterSound, base.gameObject);
		Ray aimRay = GetAimRay();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
		bullseyeSearch.maxAngleFilter = fieldOfView * 0.5f;
		bullseyeSearch.maxDistanceFilter = maxDistance;
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = aimRay.direction;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.RefreshCandidates();
		targetHurtboxes = bullseyeSearch.GetResults().Where(Util.IsValid).Distinct(default(HurtBox.EntityEqualityComparer))
			.ToList();
		totalBulletsToFire = Mathf.Max(targetHurtboxes.Count, minimumFireCount);
		timeBetweenBullets = firingDuration / (float)totalBulletsToFire;
		childLocator = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		muzzleIndex = childLocator.FindChildIndex(muzzle);
		muzzleTransform = childLocator.FindChild(muzzleIndex);
	}

	private void Fire()
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		if (totalBulletsFired >= totalBulletsToFire)
		{
			return;
		}
		if (!string.IsNullOrEmpty(muzzle))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		Util.PlaySound(fireSoundString, base.gameObject);
		PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
		if (NetworkServer.active && targetHurtboxes.Count > 0)
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = damageStat * damageCoefficient;
			damageInfo.attacker = base.gameObject;
			damageInfo.procCoefficient = procCoefficient;
			damageInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
			if (targetHurtboxIndex >= targetHurtboxes.Count)
			{
				targetHurtboxIndex = 0;
			}
			HurtBox hurtBox = targetHurtboxes[targetHurtboxIndex];
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				HealthComponent healthComponent = hurtBox.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					targetHurtboxIndex++;
					Vector3 val = ((Component)hurtBox).transform.position - base.characterBody.corePosition;
					Vector3 normalized = ((Vector3)(ref val)).normalized;
					damageInfo.force = force * normalized;
					damageInfo.position = ((Component)hurtBox).transform.position;
					EffectManager.SimpleImpactEffect(impactEffectPrefab, ((Component)hurtBox).transform.position, normalized, transmit: true);
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
				}
				if (Object.op_Implicit((Object)(object)tracerEffectPrefab) && Object.op_Implicit((Object)(object)childLocator))
				{
					int childIndex = childLocator.FindChildIndex(muzzle);
					childLocator.FindChild(childIndex);
					EffectData effectData = new EffectData
					{
						origin = ((Component)hurtBox).transform.position,
						start = muzzleTransform.position
					};
					effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
					EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
				}
			}
		}
		totalBulletsFired++;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		fireTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			Fire();
			fireTimer += timeBetweenBullets;
		}
		if (base.fixedAge >= totalDuration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
