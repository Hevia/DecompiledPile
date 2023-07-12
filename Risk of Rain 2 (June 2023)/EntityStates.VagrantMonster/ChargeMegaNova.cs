using RoR2;
using UnityEngine;

namespace EntityStates.VagrantMonster;

public class ChargeMegaNova : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargingEffectPrefab;

	public static GameObject areaIndicatorPrefab;

	public static string chargingSoundString;

	public static float novaRadius;

	private float duration;

	private float stopwatch;

	private GameObject chargeEffectInstance;

	private GameObject areaIndicatorInstance;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayCrossfade("Gesture, Override", "ChargeMegaNova", "ChargeMegaNova.playbackRate", duration, 0.3f);
		soundID = Util.PlayAttackSpeedSound(chargingSoundString, base.gameObject, attackSpeedStat);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("HullCenter");
			Transform val2 = component.FindChild("NovaCenter");
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargingEffectPrefab))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargingEffectPrefab, val.position, val.rotation);
				chargeEffectInstance.transform.localScale = new Vector3(novaRadius, novaRadius, novaRadius);
				chargeEffectInstance.transform.parent = val;
				chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
			}
			if (Object.op_Implicit((Object)(object)val2) && Object.op_Implicit((Object)(object)areaIndicatorPrefab))
			{
				areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab, val2.position, val2.rotation);
				areaIndicatorInstance.transform.localScale = new Vector3(novaRadius * 2f, novaRadius * 2f, novaRadius * 2f);
				areaIndicatorInstance.transform.parent = val2;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		AkSoundEngine.StopPlayingID(soundID);
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			EntityState.Destroy((Object)(object)areaIndicatorInstance);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			FireMegaNova fireMegaNova = new FireMegaNova();
			fireMegaNova.novaRadius = novaRadius;
			outer.SetNextState(fireMegaNova);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
