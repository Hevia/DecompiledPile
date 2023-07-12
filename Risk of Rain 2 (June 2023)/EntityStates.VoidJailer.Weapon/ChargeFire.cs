using RoR2;
using UnityEngine;

namespace EntityStates.VoidJailer.Weapon;

public class ChargeFire : BaseState
{
	public static string attackSoundEffect;

	public static string animationLayerName;

	public static string animationStateName;

	public static string animationPlaybackRateName;

	public static float baseDuration;

	public static GameObject chargeVfxPrefab;

	private float _totalDuration;

	private float _crossFadeDuration;

	private float _chargingDuration;

	private GameObject _chargeVfxInstance;

	public override void OnEnter()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		_totalDuration = baseDuration / attackSpeedStat;
		_crossFadeDuration = _totalDuration * 0.25f;
		_chargingDuration = _totalDuration - _crossFadeDuration;
		Transform modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackSoundEffect, base.gameObject, attackSpeedStat);
		if ((Object)(object)modelTransform != (Object)null)
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("ClawMuzzle");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeVfxPrefab))
				{
					_chargeVfxInstance = Object.Instantiate<GameObject>(chargeVfxPrefab, val.position, val.rotation, val);
					ScaleParticleSystemDuration component2 = _chargeVfxInstance.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.newDuration = _totalDuration;
					}
				}
			}
		}
		PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateName, _chargingDuration, _crossFadeDuration);
		base.characterBody.SetAimTimer(_totalDuration + 3f);
	}

	public override void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Object.op_Implicit((Object)(object)_chargeVfxInstance))
		{
			Ray aimRay = GetAimRay();
			_chargeVfxInstance.transform.forward = ((Ray)(ref aimRay)).direction;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= _totalDuration && base.isAuthority)
		{
			Fire nextState = new Fire();
			outer.SetNextState(nextState);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)_chargeVfxInstance))
		{
			EntityState.Destroy((Object)(object)_chargeVfxInstance);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
