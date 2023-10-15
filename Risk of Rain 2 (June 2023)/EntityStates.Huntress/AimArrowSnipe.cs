using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Huntress;

public class AimArrowSnipe : BaseArrowBarrage
{
	public static SkillDef primarySkillDef;

	public static GameObject crosshairOverridePrefab;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private GenericSkill primarySkillSlot;

	private AimAnimator modelAimAnimator;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAimAnimator = ((Component)GetModelTransform()).GetComponent<AimAnimator>();
		if (Object.op_Implicit((Object)(object)modelAimAnimator))
		{
			((Behaviour)modelAimAnimator).enabled = true;
		}
		primarySkillSlot = (Object.op_Implicit((Object)(object)base.skillLocator) ? base.skillLocator.primary : null);
		if (Object.op_Implicit((Object)(object)primarySkillSlot))
		{
			primarySkillSlot.SetSkillOverride(this, primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}
		PlayCrossfade("Body", "ArrowBarrageLoop", 0.1f);
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
	}

	protected override void HandlePrimaryAttack()
	{
		if (Object.op_Implicit((Object)(object)primarySkillSlot))
		{
			primarySkillSlot.ExecuteIfReady();
		}
	}

	public override void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = aimRay.direction;
		}
		if (!Object.op_Implicit((Object)(object)primarySkillSlot) || primarySkillSlot.stock == 0)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)primarySkillSlot))
		{
			primarySkillSlot.UnsetSkillOverride(this, primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}
}
