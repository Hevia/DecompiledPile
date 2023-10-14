using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace Rorschach;

internal class RSkillDef : SkillDef
{
	private class InstanceData : BaseSkillInstanceData
	{
		public RorschachRageBarBehaviour behaviour;
	}

	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return (BaseSkillInstanceData)(object)new InstanceData
		{
			behaviour = ((Component)skillSlot).GetComponent<RorschachRageBarBehaviour>()
		};
	}

	internal static bool IsExecutable([NotNull] GenericSkill skillSlot)
	{
		RorschachRageBarBehaviour behaviour = ((InstanceData)(object)skillSlot.skillInstanceData).behaviour;
		return behaviour.canExecute;
	}

	public override bool CanExecute([NotNull] GenericSkill skillSlot)
	{
		return IsExecutable(skillSlot) && ((SkillDef)this).CanExecute(skillSlot);
	}

	public override bool IsReady([NotNull] GenericSkill skillSlot)
	{
		return ((SkillDef)this).IsReady(skillSlot) && IsExecutable(skillSlot);
	}
}
