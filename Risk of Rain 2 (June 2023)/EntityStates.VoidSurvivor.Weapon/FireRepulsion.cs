using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor.Weapon;

public class FireRepulsion : BaseState
{
	[SerializeField]
	public string sound;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public GameObject fireEffectPrefab;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float fieldOfView;

	[SerializeField]
	public float backupDistance;

	[SerializeField]
	public float maxKnockbackDistance;

	[SerializeField]
	public float idealDistanceToPlaceTargets;

	[SerializeField]
	public float liftVelocity;

	[SerializeField]
	public float animationCrossfadeDuration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackParameterName;

	[SerializeField]
	public float damageMultiplier;

	[SerializeField]
	public float maxProjectileReflectDistance;

	[SerializeField]
	public GameObject tracerEffectPrefab;

	[SerializeField]
	public float corruption;

	[SerializeField]
	public BuffDef buffDef;

	[SerializeField]
	public float buffDuration;

	public static AnimationCurve shoveSuitabilityCurve;

	private float duration;

	private Ray aimRay;

	public override void OnEnter()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		aimRay = GetAimRay();
		PlayCrossfade(animationLayerName, animationStateName, animationPlaybackParameterName, duration, animationCrossfadeDuration);
		Util.PlaySound(sound, base.gameObject);
		Vector3 origin = aimRay.origin;
		Transform val = FindModelChild(muzzle);
		if (Object.op_Implicit((Object)(object)val))
		{
			origin = val.position;
		}
		EffectManager.SpawnEffect(fireEffectPrefab, new EffectData
		{
			origin = origin,
			rotation = Quaternion.LookRotation(aimRay.direction)
		}, transmit: false);
		ref Ray reference = ref aimRay;
		((Ray)(ref reference)).origin = ((Ray)(ref reference)).origin - aimRay.direction * backupDistance;
		if (NetworkServer.active)
		{
			PushEnemies();
			ReflectProjectiles();
			VoidSurvivorController component = GetComponent<VoidSurvivorController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.AddCorruption(corruption);
			}
		}
	}

	private void ReflectProjectiles()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (Object.op_Implicit((Object)(object)base.characterBody) ? base.characterBody.corePosition : Vector3.zero);
		TeamIndex teamIndex = (Object.op_Implicit((Object)(object)base.characterBody) ? base.characterBody.teamComponent.teamIndex : TeamIndex.None);
		float num = maxProjectileReflectDistance * maxProjectileReflectDistance;
		List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
		List<ProjectileController> list = new List<ProjectileController>();
		int i = 0;
		for (int count = instancesList.Count; i < count; i++)
		{
			ProjectileController projectileController = instancesList[i];
			if (projectileController.teamFilter.teamIndex != teamIndex)
			{
				Vector3 val2 = ((Component)projectileController).transform.position - val;
				if (((Vector3)(ref val2)).sqrMagnitude < num)
				{
					list.Add(projectileController);
				}
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			ProjectileController projectileController2 = list[j];
			if (!Object.op_Implicit((Object)(object)projectileController2))
			{
				continue;
			}
			Vector3 position = ((Component)projectileController2).transform.position;
			Vector3 start = val;
			if (Object.op_Implicit((Object)(object)tracerEffectPrefab))
			{
				EffectData effectData = new EffectData
				{
					origin = position,
					start = start
				};
				EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
			}
			_ = projectileController2.owner;
			CharacterBody component = projectileController2.owner.GetComponent<CharacterBody>();
			projectileController2.IgnoreCollisionsWithOwner(shouldIgnore: false);
			projectileController2.Networkowner = base.gameObject;
			projectileController2.teamFilter.teamIndex = base.characterBody.teamComponent.teamIndex;
			ProjectileDamage component2 = ((Component)projectileController2).GetComponent<ProjectileDamage>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.damage *= damageMultiplier;
			}
			Rigidbody component3 = ((Component)projectileController2).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				Vector3 val3 = component3.velocity * -1f;
				if (Object.op_Implicit((Object)(object)component))
				{
					val3 = component.corePosition - ((Component)component3).transform.position;
				}
				((Component)component3).transform.forward = val3;
				component3.velocity = Vector3.RotateTowards(component3.velocity, val3, float.PositiveInfinity, 0f);
			}
		}
	}

	private void PushEnemies()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.all;
		bullseyeSearch.maxAngleFilter = fieldOfView * 0.5f;
		bullseyeSearch.maxDistanceFilter = maxKnockbackDistance;
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = aimRay.direction;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		IEnumerable<HurtBox> enumerable = bullseyeSearch.GetResults().Where(Util.IsValid).Distinct(default(HurtBox.EntityEqualityComparer));
		TeamIndex team = GetTeam();
		foreach (HurtBox item in enumerable)
		{
			if (FriendlyFireManager.ShouldSplashHitProceed(item.healthComponent, team))
			{
				CharacterBody body = item.healthComponent.body;
				AddDebuff(body);
				body.RecalculateStats();
				_ = body.acceleration;
			}
		}
	}

	protected virtual void AddDebuff(CharacterBody body)
	{
		body.AddTimedBuff(buffDef, buffDuration);
		((Component)body.healthComponent).GetComponent<SetStateOnHurt>()?.SetStun(-1f);
	}

	protected virtual float CalculateDamage()
	{
		return 0f;
	}

	protected virtual float CalculateProcCoefficient()
	{
		return 0f;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
