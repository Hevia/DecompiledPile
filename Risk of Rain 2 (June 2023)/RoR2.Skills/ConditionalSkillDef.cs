using EntityStates;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Skills;

public class ConditionalSkillDef : SkillDef
{
	protected class InstanceData : BaseSkillInstanceData
	{
		public CharacterBody characterBody;
	}

	public SerializableEntityStateType baseStateType;

	public SerializableEntityStateType sprintStateType;

	public SerializableEntityStateType jumpStateType;

	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return new InstanceData
		{
			characterBody = ((Component)skillSlot).GetComponent<CharacterBody>()
		};
	}

	public bool IsGrounded([NotNull] GenericSkill skillSlot)
	{
		InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
		if (Object.op_Implicit((Object)(object)instanceData.characterBody.characterMotor) && instanceData.characterBody.characterMotor.isGrounded)
		{
			return true;
		}
		return false;
	}

	public bool IsSprinting([NotNull] GenericSkill skillSlot)
	{
		InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
		if (Object.op_Implicit((Object)(object)instanceData.characterBody) && instanceData.characterBody.isSprinting)
		{
			return true;
		}
		return false;
	}

	protected override EntityState InstantiateNextState(GenericSkill skillSlot)
	{
		bool num = IsGrounded(skillSlot);
		bool flag = IsSprinting(skillSlot);
		if (!num)
		{
			return EntityStateCatalog.InstantiateState(jumpStateType);
		}
		if (flag)
		{
			return EntityStateCatalog.InstantiateState(sprintStateType);
		}
		return EntityStateCatalog.InstantiateState(baseStateType);
	}
}
