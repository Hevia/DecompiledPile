using RoR2;
using UnityEngine;

namespace EntityStates.VagrantMonster;

public class ChargeTrackingBomb : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargingEffectPrefab;

	public static string chargingSoundString;

	private float duration;

	private float stopwatch;

	private GameObject chargeEffectInstance;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayCrossfade("Gesture, Override", "ChargeTrackingBomb", "ChargeTrackingBomb.playbackRate", duration, 0.3f);
		soundID = Util.PlayAttackSpeedSound(chargingSoundString, base.gameObject, attackSpeedStat);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("TrackingBombMuzzle");
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargingEffectPrefab))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargingEffectPrefab, val.position, val.rotation);
				chargeEffectInstance.transform.parent = val;
				chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
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
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextState(new FireTrackingBomb());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
