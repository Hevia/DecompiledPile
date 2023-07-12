using RoR2;
using UnityEngine;

namespace EntityStates.ScavMonster;

public class PrepSack : SackBaseState
{
	public static float baseDuration;

	public static string sound;

	public static GameObject chargeEffectPrefab;

	private GameObject chargeInstance;

	private float duration;

	public override void OnEnter()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Body", "PrepSack", "PrepSack.playbackRate", duration, 0.1f);
		Util.PlaySound(sound, base.gameObject);
		StartAimMode(duration);
		if (Object.op_Implicit((Object)(object)muzzleTransform) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, muzzleTransform.position, muzzleTransform.rotation);
			chargeInstance.transform.parent = muzzleTransform;
			ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new ThrowSack());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
	}
}
