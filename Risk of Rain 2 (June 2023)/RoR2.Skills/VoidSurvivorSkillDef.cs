using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Skills;

public class VoidSurvivorSkillDef : SkillDef
{
	protected class InstanceData : BaseSkillInstanceData
	{
		public VoidSurvivorController voidSurvivorController;
	}

	public float minimumCorruption;

	public float maximumCorruption;

	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return new InstanceData
		{
			voidSurvivorController = ((Component)skillSlot).GetComponent<VoidSurvivorController>()
		};
	}

	public override bool IsReady([NotNull] GenericSkill skillSlot)
	{
		if (base.IsReady(skillSlot))
		{
			return HasRequiredCorruption(skillSlot);
		}
		return false;
	}

	public bool HasRequiredCorruption([NotNull] GenericSkill skillSlot)
	{
		InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
		if (Object.op_Implicit((Object)(object)instanceData.voidSurvivorController) && instanceData.voidSurvivorController.corruption >= minimumCorruption && instanceData.voidSurvivorController.corruption < maximumCorruption)
		{
			return true;
		}
		return false;
	}
}
