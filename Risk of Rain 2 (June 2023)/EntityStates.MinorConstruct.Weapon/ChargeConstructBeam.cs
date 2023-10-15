using RoR2;
using UnityEngine;

namespace EntityStates.MinorConstruct.Weapon;

public class ChargeConstructBeam : BaseState
{
	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string exitSoundString;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public string chargeEffectMuzzle;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public float baseDuration;

	private float duration;

	private GameObject chargeInstance;

	public override void OnEnter()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		Util.PlaySound(enterSoundString, base.gameObject);
		Transform val = FindModelChild(chargeEffectMuzzle);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeInstance.transform.parent = base.gameObject.transform;
			ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
	}

	public override void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			Ray aimRay = GetAimRay();
			chargeInstance.transform.forward = aimRay.direction;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new FireConstructBeam());
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(exitSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
