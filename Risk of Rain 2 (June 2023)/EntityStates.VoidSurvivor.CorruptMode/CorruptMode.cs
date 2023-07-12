using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor.CorruptMode;

public class CorruptMode : CorruptModeBase
{
	[SerializeField]
	public SkillDef primaryOverrideSkillDef;

	[SerializeField]
	public SkillDef secondaryOverrideSkillDef;

	[SerializeField]
	public SkillDef utilityOverrideSkillDef;

	[SerializeField]
	public SkillDef specialOverrideSkillDef;

	public override void OnEnter()
	{
		base.OnEnter();
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.skillLocator))
		{
			base.skillLocator.primary.SetSkillOverride(this, primaryOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
			base.skillLocator.secondary.SetSkillOverride(this, secondaryOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
			base.skillLocator.utility.SetSkillOverride(this, utilityOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
			base.skillLocator.special.SetSkillOverride(this, specialOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
		}
		if (Object.op_Implicit((Object)(object)voidSurvivorController) && NetworkServer.active)
		{
			base.characterBody.AddBuff(voidSurvivorController.corruptedBuffDef);
		}
	}

	public override void OnExit()
	{
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.skillLocator))
		{
			base.skillLocator.primary.UnsetSkillOverride(this, primaryOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
			base.skillLocator.secondary.UnsetSkillOverride(this, secondaryOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
			base.skillLocator.utility.UnsetSkillOverride(this, utilityOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
			base.skillLocator.special.UnsetSkillOverride(this, specialOverrideSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
		}
		if (Object.op_Implicit((Object)(object)voidSurvivorController) && NetworkServer.active)
		{
			base.characterBody.RemoveBuff(voidSurvivorController.corruptedBuffDef);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && Object.op_Implicit((Object)(object)voidSurvivorController) && voidSurvivorController.corruption <= voidSurvivorController.minimumCorruption && !voidSurvivorController.isPermanentlyCorrupted && Object.op_Implicit((Object)(object)voidSurvivorController.bodyStateMachine))
		{
			voidSurvivorController.bodyStateMachine.SetInterruptState(new ExitCorruptionTransition(), InterruptPriority.Skill);
		}
	}
}
