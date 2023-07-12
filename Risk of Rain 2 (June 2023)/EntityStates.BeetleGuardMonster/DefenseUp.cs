using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BeetleGuardMonster;

public class DefenseUp : BaseState
{
	public static float baseDuration = 3.5f;

	public static float buffDuration = 8f;

	public static GameObject defenseUpPrefab;

	private Animator modelAnimator;

	private float duration;

	private bool hasCastBuff;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayCrossfade("Body", "DefenseUp", "DefenseUp.playbackRate", duration, 0.2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("DefenseUp.activate") > 0.5f && !hasCastBuff)
		{
			ScaleParticleSystemDuration component = Object.Instantiate<GameObject>(defenseUpPrefab, base.transform.position, Quaternion.identity, base.transform).GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = buffDuration;
			}
			hasCastBuff = true;
			if (NetworkServer.active)
			{
				base.characterBody.AddTimedBuff(JunkContent.Buffs.EnrageAncientWisp, buffDuration);
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
