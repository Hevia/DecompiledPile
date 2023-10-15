using RoR2;
using UnityEngine;

namespace EntityStates.VoidJailer.Weapon;

public class ChargeCapture : BaseState
{
	public static string animationLayerName;

	public static string animationStateName;

	public static string animationPlaybackRateName;

	public static float duration;

	public static string enterSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject attackIndicatorPrefab;

	public static string muzzleString;

	private float _crossFadeDuration;

	private uint soundID;

	private GameObject attackIndicatorInstance;

	public override void OnEnter()
	{
		base.OnEnter();
		duration /= attackSpeedStat;
		_crossFadeDuration = duration * 0.25f;
		PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateName, duration, _crossFadeDuration);
		soundID = Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(chargeEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)attackIndicatorPrefab))
		{
			Transform coreTransform = base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)coreTransform))
			{
				attackIndicatorInstance = Object.Instantiate<GameObject>(attackIndicatorPrefab, coreTransform);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new Capture2());
		}
	}

	public override void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)attackIndicatorInstance))
		{
			Transform obj = attackIndicatorInstance.transform;
			Ray aimRay = GetAimRay();
			obj.forward = aimRay.direction;
		}
		base.Update();
	}

	public override void OnExit()
	{
		AkSoundEngine.StopPlayingID(soundID);
		if (Object.op_Implicit((Object)(object)attackIndicatorInstance))
		{
			EntityState.Destroy((Object)(object)attackIndicatorInstance);
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
