using UnityEngine;

namespace EntityStates.ImpMonster;

public class ChargeSpines : BaseState
{
	public static float baseDuration = 1f;

	public static GameObject effectPrefab;

	private float duration;

	private GameObject chargeEffect;

	public override void OnEnter()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("MuzzleMouth");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)effectPrefab))
				{
					chargeEffect = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
					chargeEffect.transform.parent = val;
				}
			}
		}
		PlayAnimation("Gesture", "ChargeSpines", "ChargeSpines.playbackRate", duration);
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
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
			FireSpines nextState = new FireSpines();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
