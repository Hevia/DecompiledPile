using System;
using EntityStates;
using HG;

namespace RoR2.Skills;

public class ComboSkillDef : SkillDef
{
	[Serializable]
	public struct Combo
	{
		public SerializableEntityStateType activationStateType;
	}

	protected class InstanceData : BaseSkillInstanceData
	{
		public int comboCounter;
	}

	public Combo[] comboList;

	protected SerializableEntityStateType GetNextStateType(GenericSkill skillSlot)
	{
		return ArrayUtils.GetSafe<Combo>(comboList, ((InstanceData)skillSlot.skillInstanceData).comboCounter).activationStateType;
	}

	protected override EntityState InstantiateNextState(GenericSkill skillSlot)
	{
		return EntityStateCatalog.InstantiateState(GetNextStateType(skillSlot));
	}

	public override void OnExecute(GenericSkill skillSlot)
	{
		base.OnExecute(skillSlot);
		InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
		instanceData.comboCounter++;
		if (instanceData.comboCounter >= comboList.Length)
		{
			instanceData.comboCounter = 0;
		}
	}
}
