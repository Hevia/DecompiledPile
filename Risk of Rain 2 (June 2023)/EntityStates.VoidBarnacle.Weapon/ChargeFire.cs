using RoR2;
using UnityEngine;

namespace EntityStates.VoidBarnacle.Weapon;

public class ChargeFire : BaseState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public GameObject chargeVfxPrefab;

	[SerializeField]
	public string attackSoundEffect;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateName;

	private float _chargingDuration;

	private float _totalDuration;

	private float _crossFadeDuration;

	private GameObject _chargeVfxInstance;

	public override void OnEnter()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
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
				Transform val = component.FindChild("MuzzleMouth");
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
			_chargeVfxInstance.transform.forward = aimRay.direction;
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
		return InterruptPriority.Skill;
	}
}
