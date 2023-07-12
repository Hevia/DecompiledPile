using RoR2;
using UnityEngine;

namespace EntityStates.LemurianBruiserMonster;

public class ChargeMegaFireball : BaseState
{
	public static float baseDuration = 1f;

	public static GameObject chargeEffectPrefab;

	public static string attackString;

	private float duration;

	private GameObject chargeInstance;

	public override void OnEnter()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Animator modelAnimator = GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("MuzzleMouth");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
				{
					chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
					chargeInstance.transform.parent = val;
					ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.newDuration = duration;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayCrossfade("Gesture, Additive", "ChargeMegaFireball", "ChargeMegaFireball.playbackRate", duration, 0.1f);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
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
			FireMegaFireball nextState = new FireMegaFireball();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
