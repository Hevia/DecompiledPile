using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Skills;

[CreateAssetMenu(menuName = "RoR2/SkillDef/MasterSpawnSlotSkillDef")]
public class MasterSpawnSlotSkillDef : SkillDef
{
	private class InstanceData : BaseSkillInstanceData
	{
		public MasterSpawnSlotController slotController;
	}

	public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
	{
		return new InstanceData
		{
			slotController = ((Component)skillSlot).GetComponent<MasterSpawnSlotController>()
		};
	}

	private bool IsAnySlotOpen([NotNull] GenericSkill skillSlot)
	{
		if (skillSlot.skillInstanceData is InstanceData instanceData && Object.op_Implicit((Object)(object)instanceData.slotController))
		{
			return instanceData.slotController.openSlotCount > 0;
		}
		return false;
	}

	public override bool IsReady([NotNull] GenericSkill skillSlot)
	{
		if (base.IsReady(skillSlot))
		{
			return IsAnySlotOpen(skillSlot);
		}
		return false;
	}
}
