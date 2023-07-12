using RoR2;
using UnityEngine;

namespace EntityStates.VoidMegaCrab.Weapon;

public abstract class ChargeCrabCannonBase : BaseState
{
	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public string soundName;

	[SerializeField]
	public string chargeMuzzleName;

	[SerializeField]
	public string animationLayerName = "Gesture, Additive";

	[SerializeField]
	public string animationStateName = "FireCrabCannon";

	[SerializeField]
	public string animationStateNamePreCharged = "FireCrabCannon";

	[SerializeField]
	public string animationPlaybackRateParam = "FireCrabCannon.playbackRate";

	[SerializeField]
	public float animationCrossfadeDuration = 0.2f;

	private GameObject chargeInstance;

	private float duration;

	public override void OnEnter()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex(animationLayerName);
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Empty"))
			{
				PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateParam, duration, animationCrossfadeDuration);
			}
			else
			{
				PlayCrossfade(animationLayerName, animationStateNamePreCharged, animationPlaybackRateParam, duration, animationCrossfadeDuration);
			}
		}
		Util.PlaySound(soundName, base.gameObject);
		Transform val = FindModelChild(chargeMuzzleName);
		if (Object.op_Implicit((Object)(object)chargeEffectPrefab) && Object.op_Implicit((Object)(object)val))
		{
			chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeInstance.transform.parent = val;
			ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
		base.characterBody.SetAimTimer(duration + 2f);
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(GetNextState());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	protected abstract FireCrabCannonBase GetNextState();
}
