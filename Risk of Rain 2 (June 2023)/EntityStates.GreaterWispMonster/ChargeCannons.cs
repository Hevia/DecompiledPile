using RoR2;
using UnityEngine;

namespace EntityStates.GreaterWispMonster;

public class ChargeCannons : BaseState
{
	[SerializeField]
	public float baseDuration = 3f;

	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public string attackString;

	protected float duration;

	private GameObject chargeEffectLeft;

	private GameObject chargeEffectRight;

	private const float soundDuration = 2f;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		soundID = Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat * (2f / baseDuration));
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		GetModelAnimator();
		PlayAnimation("Gesture", "ChargeCannons", "ChargeCannons.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)effectPrefab))
			{
				Transform val = component.FindChild("MuzzleLeft");
				Transform val2 = component.FindChild("MuzzleRight");
				if (Object.op_Implicit((Object)(object)val))
				{
					chargeEffectLeft = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
					chargeEffectLeft.transform.parent = val;
					ScaleParticleSystemDuration component2 = chargeEffectLeft.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.newDuration = duration;
					}
				}
				if (Object.op_Implicit((Object)(object)val2))
				{
					chargeEffectRight = Object.Instantiate<GameObject>(effectPrefab, val2.position, val2.rotation);
					chargeEffectRight.transform.parent = val2;
					ScaleParticleSystemDuration component3 = chargeEffectRight.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component3))
					{
						component3.newDuration = duration;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		if (base.fixedAge < duration - 0.1f)
		{
			AkSoundEngine.StopPlayingID(soundID);
		}
		PlayAnimation("Gesture", "Empty");
		EntityState.Destroy((Object)(object)chargeEffectLeft);
		EntityState.Destroy((Object)(object)chargeEffectRight);
		base.OnExit();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireCannons nextState = new FireCannons();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
