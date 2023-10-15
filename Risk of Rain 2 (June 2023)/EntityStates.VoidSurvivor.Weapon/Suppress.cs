using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor.Weapon;

public class Suppress : BaseSkillState
{
	[SerializeField]
	public float minimumDuration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public float maxSearchAngleFilter;

	[SerializeField]
	public float maxSearchDistance;

	[SerializeField]
	public float idealDistance;

	[SerializeField]
	public float springConstant;

	[SerializeField]
	public float springMaxLength;

	[SerializeField]
	public float damperConstant;

	[SerializeField]
	public float suppressedTargetForceRadius;

	[SerializeField]
	public GameObject suppressEffectPrefab;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public AnimationCurve forceSuitabilityCurve;

	[SerializeField]
	public float damageCoefficientPerSecond;

	[SerializeField]
	public float procCoefficientPerSecond;

	[SerializeField]
	public float corruptionPerSecond;

	[SerializeField]
	public float maxBreakDistance;

	[SerializeField]
	public float tickRate;

	[SerializeField]
	public bool applyForces;

	private GameObject suppressEffectInstance;

	private Transform idealFXTransform;

	private Transform targetFXTransform;

	private Transform muzzleTransform;

	private VoidSurvivorController voidSurvivorController;

	private CharacterBody targetBody;

	private float damageTickCountdown;

	public override void OnEnter()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		targetBody = null;
		voidSurvivorController = GetComponent<VoidSurvivorController>();
		PlayAnimation(animationLayerName, animationStateName);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				muzzleTransform = component.FindChild(muzzle);
				if (Object.op_Implicit((Object)(object)muzzleTransform) && Object.op_Implicit((Object)(object)suppressEffectPrefab))
				{
					suppressEffectInstance = Object.Instantiate<GameObject>(suppressEffectPrefab, muzzleTransform.position, muzzleTransform.rotation);
					suppressEffectInstance.transform.parent = ((Component)base.characterBody).transform;
					ChildLocator component2 = suppressEffectInstance.GetComponent<ChildLocator>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						idealFXTransform = component2.FindChild("IdealFX");
						targetFXTransform = component2.FindChild("TargetFX");
					}
				}
			}
		}
		Ray aimRay = GetAimRay();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
		bullseyeSearch.maxAngleFilter = maxSearchAngleFilter;
		bullseyeSearch.maxDistanceFilter = maxSearchDistance;
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = aimRay.direction;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			Debug.LogFormat("Found target {0}", new object[1] { targetBody });
			targetBody = hurtBox.healthComponent.body;
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)suppressEffectInstance))
		{
			EntityState.Destroy((Object)(object)suppressEffectInstance);
		}
		base.OnExit();
	}

	public override void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			if (Object.op_Implicit((Object)(object)muzzleTransform))
			{
				suppressEffectInstance.transform.position = muzzleTransform.position;
			}
			if (Object.op_Implicit((Object)(object)idealFXTransform))
			{
				Ray aimRay = GetAimRay();
				_ = targetBody.corePosition;
				Vector3 position = aimRay.origin + aimRay.direction * idealDistance;
				idealFXTransform.position = position;
			}
			if (Object.op_Implicit((Object)(object)targetFXTransform))
			{
				targetFXTransform.position = targetBody.corePosition;
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		base.characterBody.SetAimTimer(3f);
		if (NetworkServer.active && Object.op_Implicit((Object)(object)targetBody))
		{
			Ray aimRay = GetAimRay();
			Vector3 corePosition = targetBody.corePosition;
			Vector3 val = aimRay.origin + aimRay.direction * idealDistance;
			if (applyForces)
			{
				Vector3 val2 = val - corePosition;
				float magnitude = ((Vector3)(ref val2)).magnitude;
				Mathf.Clamp01(magnitude / suppressedTargetForceRadius);
				Vector3 velocity;
				float mass;
				bool flag;
				if (Object.op_Implicit((Object)(object)targetBody.characterMotor))
				{
					velocity = targetBody.characterMotor.velocity;
					mass = targetBody.characterMotor.mass;
					flag = !targetBody.characterMotor.isFlying;
				}
				else
				{
					Rigidbody obj = targetBody.rigidbody;
					velocity = obj.velocity;
					mass = obj.mass;
					flag = obj.useGravity;
				}
				Vector3 val3 = ((Vector3)(ref val2)).normalized * Mathf.Min(springMaxLength, magnitude) * springConstant * Time.fixedDeltaTime;
				Vector3 val4 = -velocity * damperConstant * Time.fixedDeltaTime;
				Vector3 val5 = (flag ? (Physics.gravity * Time.fixedDeltaTime * mass) : Vector3.zero);
				float num = forceSuitabilityCurve.Evaluate(mass);
				targetBody.healthComponent.TakeDamageForce((val3 + val4) * num - val5, alwaysApply: true, disableAirControlUntilCollision: true);
			}
			damageTickCountdown -= Time.fixedDeltaTime;
			if (damageTickCountdown <= 0f)
			{
				damageTickCountdown = 1f / tickRate;
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.attacker = base.gameObject;
				damageInfo.procCoefficient = procCoefficientPerSecond / tickRate;
				damageInfo.damage = damageCoefficientPerSecond * damageStat / tickRate;
				damageInfo.crit = RollCrit();
				targetBody.healthComponent.TakeDamage(damageInfo);
				voidSurvivorController.AddCorruption(corruptionPerSecond / tickRate);
			}
		}
		if (base.isAuthority && (!Object.op_Implicit((Object)(object)targetBody) || !targetBody.healthComponent.alive))
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
