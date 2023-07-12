using RoR2;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Weapon;

public class ChargeMissiles : BaseState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public bool isSoundScaledByAttackSpeed;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private float duration;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			Transform val = modelChildLocator.FindChild(muzzleName) ?? base.characterBody.coreTransform;
			if (Object.op_Implicit((Object)(object)val))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
				chargeEffectInstance.transform.parent = val;
			}
		}
		if (!string.IsNullOrEmpty(enterSoundString))
		{
			if (isSoundScaledByAttackSpeed)
			{
				Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
			}
			else
			{
				Util.PlaySound(enterSoundString, base.gameObject);
			}
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)chargeEffectInstance);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new FireMissiles());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
