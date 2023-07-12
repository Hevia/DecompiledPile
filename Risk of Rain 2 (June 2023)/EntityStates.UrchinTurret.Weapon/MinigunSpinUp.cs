using RoR2;
using UnityEngine;

namespace EntityStates.UrchinTurret.Weapon;

public class MinigunSpinUp : MinigunState
{
	public static float baseDuration;

	public static string sound;

	public static GameObject chargeEffectPrefab;

	private GameObject chargeInstance;

	private float duration;

	public override void OnEnter()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(sound, base.gameObject);
		PlayCrossfade("Gesture, Additive", "EnterShootLoop", 0.2f);
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
			outer.SetNextState(new MinigunFire());
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
