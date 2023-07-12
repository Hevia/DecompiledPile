using EntityStates;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Skills;

[CreateAssetMenu(menuName = "RoR2/SkillDef/VoidRaidCrabWeaponSkillDef")]
public class VoidRaidCrabWeaponSkillDef : SkillDef
{
	private class InstanceData : BaseSkillInstanceData
	{
		public EntityStateMachine bodyStateMachine;

		public EntityStateMachine weaponStateMachine;
	}

	private const string bodyStateMachineName = "Body";

	private const string weaponStateMachineName = "Weapon";

	public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
	{
		return new InstanceData
		{
			bodyStateMachine = EntityStateMachine.FindByCustomName(((Component)skillSlot).gameObject, "Body"),
			weaponStateMachine = EntityStateMachine.FindByCustomName(((Component)skillSlot).gameObject, "Weapon")
		};
	}

	public override bool IsReady([NotNull] GenericSkill skillSlot)
	{
		if (base.IsReady(skillSlot) && CanReceiveInput(skillSlot))
		{
			return !IsWeaponBusy(skillSlot);
		}
		return false;
	}

	private bool CanReceiveInput([NotNull] GenericSkill skillSlot)
	{
		if (skillSlot.skillInstanceData is InstanceData instanceData && Object.op_Implicit((Object)(object)instanceData.bodyStateMachine))
		{
			return instanceData.bodyStateMachine.state is GenericCharacterMain;
		}
		return false;
	}

	private bool IsWeaponBusy([NotNull] GenericSkill skillSlot)
	{
		if (skillSlot.skillInstanceData is InstanceData instanceData && Object.op_Implicit((Object)(object)instanceData.weaponStateMachine))
		{
			return !instanceData.weaponStateMachine.IsInMainState();
		}
		return false;
	}
}
