using RoR2;
using UnityEngine;

namespace EntityStates.GravekeeperBoss;

public class PrepHook : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargeEffectPrefab;

	public static string muzzleString;

	public static string attackString;

	private float duration;

	private GameObject chargeInstance;

	private Animator modelAnimator;

	public override void OnEnter()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		base.fixedAge = 0f;
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			PlayCrossfade("Body", "PrepHook", "PrepHook.playbackRate", duration, 0.5f);
			((Behaviour)((Component)modelAnimator).GetComponent<AimAnimator>()).enabled = true;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.inputBank.aimDirection;
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild(muzzleString);
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
		Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new FireHook());
		}
	}

	public override void OnExit()
	{
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
