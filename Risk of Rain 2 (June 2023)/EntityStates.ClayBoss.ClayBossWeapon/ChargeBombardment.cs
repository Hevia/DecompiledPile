using RoR2;
using UnityEngine;

namespace EntityStates.ClayBoss.ClayBossWeapon;

public class ChargeBombardment : BaseState
{
	public static float baseTotalDuration;

	public static float baseMaxChargeTime;

	public static int maxCharges;

	public static GameObject chargeEffectPrefab;

	public static string chargeLoopStartSoundString;

	public static string chargeLoopStopSoundString;

	public static int minGrenadeCount;

	public static int maxGrenadeCount;

	public static float minBonusBloom;

	public static float maxBonusBloom;

	private float stopwatch;

	private GameObject chargeInstance;

	private int charge;

	private float totalDuration;

	private float maxChargeTime;

	public override void OnEnter()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		totalDuration = baseTotalDuration / attackSpeedStat;
		maxChargeTime = baseMaxChargeTime / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayAnimation("Gesture, Additive", "ChargeBombardment");
		Util.PlaySound(chargeLoopStartSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("Muzzle");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
				{
					chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
					chargeInstance.transform.parent = val;
					ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.newDuration = totalDuration;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(totalDuration);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayAnimation("Gesture, Additive", "Empty");
		Util.PlaySound(chargeLoopStopSoundString, base.gameObject);
		EntityState.Destroy((Object)(object)chargeInstance);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		charge = Mathf.Min((int)(stopwatch / maxChargeTime * (float)maxCharges), maxCharges);
		float num = (float)charge / (float)maxCharges;
		float value = Mathf.Lerp(minBonusBloom, maxBonusBloom, num);
		base.characterBody.SetSpreadBloom(value);
		int grenadeCountMax = Mathf.FloorToInt(Mathf.Lerp((float)minGrenadeCount, (float)maxGrenadeCount, num));
		if ((stopwatch >= totalDuration || !Object.op_Implicit((Object)(object)base.inputBank) || !base.inputBank.skill1.down) && base.isAuthority)
		{
			FireBombardment fireBombardment = new FireBombardment();
			fireBombardment.grenadeCountMax = grenadeCountMax;
			outer.SetNextState(fireBombardment);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
