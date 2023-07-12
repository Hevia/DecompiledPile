using RoR2;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Railgunner.Weapon;

public abstract class BaseWindupSnipe : BaseState, IBaseWeaponState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public GameObject crosshairOverridePrefab;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public GameObject windupEffectPrefab;

	[SerializeField]
	public string windupEffectMuzzle;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private GameObject windupEffectInstance;

	public override void OnEnter()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		if (Object.op_Implicit((Object)(object)windupEffectPrefab))
		{
			Transform val = FindModelChild(windupEffectMuzzle);
			if (Object.op_Implicit((Object)(object)val))
			{
				windupEffectInstance = Object.Instantiate<GameObject>(windupEffectPrefab, val.position, val.rotation);
				windupEffectInstance.transform.parent = val;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		base.characterBody.SetSpreadBloom(base.age / duration, canOnlyIncreaseBloom: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(InstantiateNextState());
		}
	}

	public override void OnExit()
	{
		crosshairOverrideRequest?.Dispose();
		if (Object.op_Implicit((Object)(object)windupEffectInstance))
		{
			EntityState.Destroy((Object)(object)windupEffectInstance);
		}
		base.OnExit();
	}

	protected abstract EntityState InstantiateNextState();

	public bool CanScope()
	{
		return true;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
