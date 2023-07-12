using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Railgunner.Backpack;

public abstract class BaseOnline : BaseBackpack
{
	[SerializeField]
	public SkillDef requiredSkillDef;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string cooldownParamName;

	private Animator animator;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName);
		animator = GetModelAnimator();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.skillLocator.special.skillDef != requiredSkillDef)
		{
			outer.SetNextState(new Disconnected());
		}
		else if (Object.op_Implicit((Object)(object)animator))
		{
			float num = base.skillLocator.special.CalculateFinalRechargeInterval();
			float num2 = 0f;
			if (num > 0f)
			{
				num2 = base.skillLocator.special.cooldownRemaining / num;
			}
			animator.SetFloat(cooldownParamName, num2);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetFloat(cooldownParamName, 0f);
		}
		base.OnExit();
	}
}
