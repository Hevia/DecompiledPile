using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class ChargeSpear : BaseToolbotPrimarySkillState
{
	public static float baseMinChargeDuration;

	public static float baseChargeDuration;

	public static float perfectChargeWindow;

	public new static string muzzleName;

	public static GameObject chargeupVfxPrefab;

	public static GameObject holdChargeVfxPrefab;

	private float minChargeDuration;

	private float chargeDuration;

	private bool released;

	private GameObject chargeupVfxGameObject;

	private GameObject holdChargeVfxGameObject;

	public override void OnEnter()
	{
		base.OnEnter();
		minChargeDuration = baseMinChargeDuration / attackSpeedStat;
		chargeDuration = baseChargeDuration / attackSpeedStat;
		if (!base.isInDualWield)
		{
			PlayAnimation("Gesture, Additive", "ChargeSpear", "ChargeSpear.playbackRate", chargeDuration);
		}
		if (Object.op_Implicit((Object)(object)base.muzzleTransform))
		{
			chargeupVfxGameObject = Object.Instantiate<GameObject>(chargeupVfxPrefab, base.muzzleTransform);
			chargeupVfxGameObject.GetComponent<ScaleParticleSystemDuration>().newDuration = chargeDuration;
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeupVfxGameObject))
		{
			EntityState.Destroy((Object)(object)chargeupVfxGameObject);
			chargeupVfxGameObject = null;
		}
		if (Object.op_Implicit((Object)(object)holdChargeVfxGameObject))
		{
			EntityState.Destroy((Object)(object)holdChargeVfxGameObject);
			holdChargeVfxGameObject = null;
		}
		base.OnExit();
	}

	public override void Update()
	{
		base.Update();
		base.characterBody.SetSpreadBloom(base.age / chargeDuration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		float num = base.fixedAge - chargeDuration;
		if (num >= 0f)
		{
			_ = perfectChargeWindow;
		}
		float charge = Mathf.Clamp01(base.fixedAge / chargeDuration);
		if (base.fixedAge >= chargeDuration)
		{
			if (Object.op_Implicit((Object)(object)chargeupVfxGameObject))
			{
				EntityState.Destroy((Object)(object)chargeupVfxGameObject);
				chargeupVfxGameObject = null;
			}
			if (!Object.op_Implicit((Object)(object)holdChargeVfxGameObject) && Object.op_Implicit((Object)(object)base.muzzleTransform))
			{
				holdChargeVfxGameObject = Object.Instantiate<GameObject>(holdChargeVfxPrefab, base.muzzleTransform);
			}
		}
		if (base.isAuthority)
		{
			if (!released && !IsKeyDownAuthority())
			{
				released = true;
			}
			if (released && base.fixedAge >= minChargeDuration)
			{
				outer.SetNextState(new FireSpear
				{
					charge = charge,
					activatorSkillSlot = base.activatorSkillSlot
				});
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
