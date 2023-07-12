using UnityEngine;

namespace EntityStates.AncientWispMonster;

public class ChargeRHCannon : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject effectPrefab;

	private float duration;

	private GameObject chargeEffectLeft;

	private GameObject chargeEffectRight;

	public override void OnEnter()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayAnimation("Gesture", "ChargeRHCannon", "ChargeRHCannon.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)effectPrefab))
			{
				Transform val = component.FindChild("MuzzleRight");
				if (Object.op_Implicit((Object)(object)val))
				{
					chargeEffectRight = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
					chargeEffectRight.transform.parent = val;
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
		base.OnExit();
		EntityState.Destroy((Object)(object)chargeEffectLeft);
		EntityState.Destroy((Object)(object)chargeEffectRight);
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
			FireRHCannon nextState = new FireRHCannon();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
