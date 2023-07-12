using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Treebot.Weapon;

public class FireSonicBoom : BaseState
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
	public float maxDistance;

	[SerializeField]
	public float idealDistanceToPlaceTargets;

	[SerializeField]
	public float liftVelocity;

	[SerializeField]
	public float slowDuration;

	[SerializeField]
	public float groundKnockbackDistance;

	[SerializeField]
	public float airKnockbackDistance;

	public static AnimationCurve shoveSuitabilityCurve;

	private float duration;

	public override void OnEnter()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "FireSonicBoom");
		Util.PlaySound(sound, base.gameObject);
		Ray aimRay = GetAimRay();
		if (!string.IsNullOrEmpty(muzzle))
		{
			EffectManager.SimpleMuzzleFlash(fireEffectPrefab, base.gameObject, muzzle, transmit: false);
		}
		else
		{
			EffectManager.SpawnEffect(fireEffectPrefab, new EffectData
			{
				origin = ((Ray)(ref aimRay)).origin,
				rotation = Quaternion.LookRotation(((Ray)(ref aimRay)).direction)
			}, transmit: false);
		}
		((Ray)(ref aimRay)).origin = ((Ray)(ref aimRay)).origin - ((Ray)(ref aimRay)).direction * backupDistance;
		if (NetworkServer.active)
		{
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.teamMaskFilter = TeamMask.all;
			bullseyeSearch.maxAngleFilter = fieldOfView * 0.5f;
			bullseyeSearch.maxDistanceFilter = maxDistance;
			bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
			bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
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
					Vector3 val = ((Component)item).transform.position - ((Ray)(ref aimRay)).origin;
					float magnitude = ((Vector3)(ref val)).magnitude;
					Vector2 val2 = new Vector2(val.x, val.z);
					_ = ((Vector2)(ref val2)).magnitude;
					Vector3 val3 = val / magnitude;
					float num = 1f;
					CharacterBody body = item.healthComponent.body;
					if (Object.op_Implicit((Object)(object)body.characterMotor))
					{
						num = body.characterMotor.mass;
					}
					else if (Object.op_Implicit((Object)(object)((Component)item.healthComponent).GetComponent<Rigidbody>()))
					{
						num = base.rigidbody.mass;
					}
					float num2 = shoveSuitabilityCurve.Evaluate(num);
					AddDebuff(body);
					body.RecalculateStats();
					float acceleration = body.acceleration;
					Vector3 val4 = val3;
					float num3 = Trajectory.CalculateInitialYSpeedForHeight(Mathf.Abs(idealDistanceToPlaceTargets - magnitude), 0f - acceleration) * Mathf.Sign(idealDistanceToPlaceTargets - magnitude);
					val4 *= num3;
					val4.y = liftVelocity;
					DamageInfo damageInfo = new DamageInfo
					{
						attacker = base.gameObject,
						damage = CalculateDamage(),
						position = ((Component)item).transform.position,
						procCoefficient = CalculateProcCoefficient()
					};
					item.healthComponent.TakeDamageForce(val4 * (num * num2), alwaysApply: true, disableAirControlUntilCollision: true);
					item.healthComponent.TakeDamage(new DamageInfo
					{
						attacker = base.gameObject,
						damage = CalculateDamage(),
						position = ((Component)item).transform.position,
						procCoefficient = CalculateProcCoefficient()
					});
					GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)item.healthComponent).gameObject);
				}
			}
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterBody) && Object.op_Implicit((Object)(object)base.characterBody.characterMotor))
		{
			float height = (base.characterBody.characterMotor.isGrounded ? groundKnockbackDistance : airKnockbackDistance);
			float num4 = (Object.op_Implicit((Object)(object)base.characterBody.characterMotor) ? base.characterBody.characterMotor.mass : 1f);
			float acceleration2 = base.characterBody.acceleration;
			float num5 = Trajectory.CalculateInitialYSpeedForHeight(height, 0f - acceleration2);
			base.characterBody.characterMotor.ApplyForce((0f - num5) * num4 * ((Ray)(ref aimRay)).direction);
		}
	}

	protected virtual void AddDebuff(CharacterBody body)
	{
		body.AddTimedBuff(RoR2Content.Buffs.Weak, slowDuration);
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
