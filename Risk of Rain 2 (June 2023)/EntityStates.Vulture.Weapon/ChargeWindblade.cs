using RoR2;
using UnityEngine;

namespace EntityStates.Vulture.Weapon;

public class ChargeWindblade : BaseSkillState
{
	public static float baseDuration;

	public static string muzzleString;

	public static GameObject chargeEffectPrefab;

	public static string soundString;

	private float duration;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "ChargeWindblade", "ChargeWindblade.playbackRate", duration);
		Util.PlaySound(soundString, base.gameObject);
		base.characterBody.SetAimTimer(3f);
		Transform val = FindModelChild(muzzleString);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeEffectInstance.transform.parent = val;
			ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new FireWindblade());
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		base.OnExit();
	}
}
