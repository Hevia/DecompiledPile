using RoR2;
using UnityEngine;

namespace EntityStates.ScavMonster;

public class PrepEnergyCannon : EnergyCannonState
{
	public static float baseDuration;

	public static string sound;

	public static GameObject chargeEffectPrefab;

	private GameObject chargeInstance;

	private float duration;

	public override void OnEnter()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Body", "PrepEnergyCannon", "PrepEnergyCannon.playbackRate", duration, 0.1f);
		Util.PlaySound(sound, base.gameObject);
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
		StartAimMode(0.5f);
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new FireEnergyCannon());
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
