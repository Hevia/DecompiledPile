using RoR2;
using UnityEngine;

namespace EntityStates.LemurianMonster;

public class ChargeFireball : BaseState
{
	public static float baseDuration = 1f;

	public static GameObject chargeVfxPrefab;

	public static string attackString;

	private float duration;

	private GameObject chargeVfxInstance;

	public override void OnEnter()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("MuzzleMouth");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeVfxPrefab))
				{
					chargeVfxInstance = Object.Instantiate<GameObject>(chargeVfxPrefab, val.position, val.rotation);
					chargeVfxInstance.transform.parent = val;
				}
			}
		}
		PlayAnimation("Gesture", "ChargeFireball", "ChargeFireball.playbackRate", duration);
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeVfxInstance))
		{
			EntityState.Destroy((Object)(object)chargeVfxInstance);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireFireball nextState = new FireFireball();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
