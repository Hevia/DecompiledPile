using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Skills;

public class GroundedSkillDef : SkillDef
{
	protected class InstanceData : BaseSkillInstanceData
	{
		public CharacterMotor characterMotor;
	}

	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return new InstanceData
		{
			characterMotor = ((Component)skillSlot).GetComponent<CharacterMotor>()
		};
	}

	public bool IsGrounded([NotNull] GenericSkill skillSlot)
	{
		InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
		if (Object.op_Implicit((Object)(object)instanceData.characterMotor) && instanceData.characterMotor.isGrounded)
		{
			return true;
		}
		return false;
	}

	public override bool IsReady([NotNull] GenericSkill skillSlot)
	{
		if (HasRequiredStockAndDelay(skillSlot))
		{
			return IsGrounded(skillSlot);
		}
		return false;
	}
}
