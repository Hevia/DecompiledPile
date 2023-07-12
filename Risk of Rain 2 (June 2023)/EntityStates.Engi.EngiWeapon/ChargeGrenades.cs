using RoR2;
using UnityEngine;

namespace EntityStates.Engi.EngiWeapon;

public class ChargeGrenades : BaseState
{
	public static float baseTotalDuration;

	public static float baseMaxChargeTime;

	public static int maxCharges;

	public static GameObject chargeEffectPrefab;

	public static string chargeStockSoundString;

	public static string chargeLoopStartSoundString;

	public static string chargeLoopStopSoundString;

	public static int minGrenadeCount;

	public static int maxGrenadeCount;

	public static float minBonusBloom;

	public static float maxBonusBloom;

	private GameObject chargeLeftInstance;

	private GameObject chargeRightInstance;

	private int charge;

	private int lastCharge;

	private float totalDuration;

	private float maxChargeTime;

	public override void OnEnter()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		totalDuration = baseTotalDuration / attackSpeedStat;
		maxChargeTime = baseMaxChargeTime / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayAnimation("Gesture, Additive", "ChargeGrenades");
		Util.PlaySound(chargeLoopStartSoundString, base.gameObject);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform val = component.FindChild("MuzzleLeft");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeLeftInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeLeftInstance.transform.parent = val;
			ScaleParticleSystemDuration component2 = chargeLeftInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.newDuration = totalDuration;
			}
		}
		Transform val2 = component.FindChild("MuzzleRight");
		if (Object.op_Implicit((Object)(object)val2) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeRightInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val2.position, val2.rotation);
			chargeRightInstance.transform.parent = val2;
			ScaleParticleSystemDuration component3 = chargeRightInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.newDuration = totalDuration;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayAnimation("Gesture, Additive", "Empty");
		Util.PlaySound(chargeLoopStopSoundString, base.gameObject);
		EntityState.Destroy((Object)(object)chargeLeftInstance);
		EntityState.Destroy((Object)(object)chargeRightInstance);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		lastCharge = charge;
		charge = Mathf.Min((int)(base.fixedAge / maxChargeTime * (float)maxCharges), maxCharges);
		float num = (float)charge / (float)maxCharges;
		float value = Mathf.Lerp(minBonusBloom, maxBonusBloom, num);
		base.characterBody.SetSpreadBloom(value);
		int num2 = Mathf.FloorToInt(Mathf.Lerp((float)minGrenadeCount, (float)maxGrenadeCount, num));
		if (lastCharge < charge)
		{
			Util.PlaySound(chargeStockSoundString, base.gameObject, "engiM1_chargePercent", 100f * ((float)(num2 - 1) / (float)maxGrenadeCount));
		}
		if ((base.fixedAge >= totalDuration || !Object.op_Implicit((Object)(object)base.inputBank) || !base.inputBank.skill1.down) && base.isAuthority)
		{
			FireGrenades fireGrenades = new FireGrenades();
			fireGrenades.grenadeCountMax = num2;
			outer.SetNextState(fireGrenades);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
