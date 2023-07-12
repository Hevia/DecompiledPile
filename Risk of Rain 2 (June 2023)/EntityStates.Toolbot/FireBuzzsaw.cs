using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class FireBuzzsaw : BaseToolbotPrimarySkillState
{
	public static float damageCoefficientPerSecond;

	public static float procCoefficientPerSecond = 1f;

	public static string fireSoundString;

	public static string impactSoundString;

	public static string spinUpSoundString;

	public static string spinDownSoundString;

	public static float spreadBloomValue = 0.2f;

	public static float baseFireFrequency;

	public static GameObject spinEffectPrefab;

	public static GameObject spinImpactEffectPrefab;

	public static GameObject impactEffectPrefab;

	public static float selfForceMagnitude;

	private OverlapAttack attack;

	private float fireFrequency;

	private float fireAge;

	private GameObject spinEffectInstance;

	private GameObject spinImpactEffectInstance;

	private bool hitOverlapLastTick;

	public override string baseMuzzleName => "MuzzleBuzzsaw";

	public override void OnEnter()
	{
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		fireFrequency = baseFireFrequency * attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		Util.PlaySound(spinUpSoundString, base.gameObject);
		Util.PlaySound(fireSoundString, base.gameObject);
		if (!base.isInDualWield)
		{
			PlayAnimation("Gesture, Additive Gun", "SpinBuzzsaw");
			PlayAnimation("Gesture, Additive", "EnterBuzzsaw");
		}
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = damageCoefficientPerSecond * damageStat / baseFireFrequency;
		attack.procCoefficient = procCoefficientPerSecond / baseFireFrequency;
		if (Object.op_Implicit((Object)(object)impactEffectPrefab))
		{
			attack.hitEffectPrefab = impactEffectPrefab;
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			string groupName = "Buzzsaw";
			if (base.isInDualWield)
			{
				if (base.currentHand == -1)
				{
					groupName = "BuzzsawL";
				}
				else if (base.currentHand == 1)
				{
					groupName = "BuzzsawR";
				}
			}
			attack.hitBoxGroup = HitBoxGroup.FindByGroupName(((Component)modelTransform).gameObject, groupName);
		}
		if (Object.op_Implicit((Object)(object)base.muzzleTransform))
		{
			if (Object.op_Implicit((Object)(object)spinEffectPrefab))
			{
				spinEffectInstance = Object.Instantiate<GameObject>(spinEffectPrefab, base.muzzleTransform.position, base.muzzleTransform.rotation);
				spinEffectInstance.transform.parent = base.muzzleTransform;
				spinEffectInstance.transform.localScale = Vector3.one;
			}
			if (Object.op_Implicit((Object)(object)spinImpactEffectPrefab))
			{
				spinImpactEffectInstance = Object.Instantiate<GameObject>(spinImpactEffectPrefab, base.muzzleTransform.position, base.muzzleTransform.rotation);
				spinImpactEffectInstance.transform.parent = base.muzzleTransform;
				spinImpactEffectInstance.transform.localScale = Vector3.one;
				spinImpactEffectInstance.gameObject.SetActive(false);
			}
		}
		attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
	}

	public override void OnExit()
	{
		base.OnExit();
		Util.PlaySound(spinDownSoundString, base.gameObject);
		if (!base.isInDualWield)
		{
			PlayAnimation("Gesture, Additive Gun", "Empty");
			PlayAnimation("Gesture, Additive", "ExitBuzzsaw");
		}
		if (Object.op_Implicit((Object)(object)spinEffectInstance))
		{
			EntityState.Destroy((Object)(object)spinEffectInstance);
		}
		if (Object.op_Implicit((Object)(object)spinImpactEffectInstance))
		{
			EntityState.Destroy((Object)(object)spinImpactEffectInstance);
		}
	}

	public override void FixedUpdate()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		fireAge += Time.fixedDeltaTime;
		base.characterBody.SetAimTimer(2f);
		attackSpeedStat = base.characterBody.attackSpeed;
		fireFrequency = baseFireFrequency * attackSpeedStat;
		if (fireAge >= 1f / fireFrequency && base.isAuthority)
		{
			fireAge = 0f;
			attack.ResetIgnoredHealthComponents();
			attack.isCrit = base.characterBody.RollCrit();
			hitOverlapLastTick = attack.Fire();
			if (hitOverlapLastTick)
			{
				Vector3 lastFireAverageHitPosition = attack.lastFireAverageHitPosition;
				Ray aimRay = GetAimRay();
				Vector3 val = lastFireAverageHitPosition - ((Ray)(ref aimRay)).origin;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				if (Object.op_Implicit((Object)(object)base.characterMotor))
				{
					base.characterMotor.ApplyForce(normalized * selfForceMagnitude);
				}
				Util.PlaySound(impactSoundString, base.gameObject);
				if (!base.isInDualWield)
				{
					PlayAnimation("Gesture, Additive", "ImpactBuzzsaw");
				}
			}
			base.characterBody.AddSpreadBloom(spreadBloomValue);
			if (!IsKeyDownAuthority() || base.skillDef != base.activatorSkillSlot.skillDef)
			{
				outer.SetNextStateToMain();
			}
		}
		spinImpactEffectInstance.gameObject.SetActive(hitOverlapLastTick);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
